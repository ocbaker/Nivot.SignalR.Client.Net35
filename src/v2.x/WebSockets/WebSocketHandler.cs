using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Infrastructure;
using Microsoft.AspNet.SignalR.Client.Transports.WebSockets;
using WebSocketSharp;
using Rackspace.Threading;

namespace Microsoft.AspNet.SignalR.Client.WebSockets
{
	public class WebSocketHandler
	{
		// Wait 250 ms before giving up on a Close
		private static readonly TimeSpan _closeTimeout = TimeSpan.FromMilliseconds(250);

		// 4KB default fragment size (we expect most messages to be very short)
		private const int _receiveLoopBufferSize = 4 * 1024;
		private readonly int? _maxIncomingMessageSize;

		// Queue for sending messages
		private readonly TaskQueue _sendQueue = new TaskQueue();

		public WebSocketHandler(int? maxIncomingMessageSize)
		{
			_maxIncomingMessageSize = maxIncomingMessageSize;
		}

		public virtual void OnOpen() { }

		public virtual void OnMessage(string message) { throw new NotImplementedException(); }

		public virtual void OnMessage(byte[] message) { throw new NotImplementedException(); }

		public virtual void OnError() { }

		public virtual void OnClose() { }

		// Sends a text message to the client
		public virtual Task Send(IConnection connection, string message)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}

			return SendAsync(connection, message);
		}

		internal Task SendAsync(IConnection connection, string message)
		{
			var buffer = Encoding.UTF8.GetBytes(message);

			return _sendQueue.Enqueue(state =>
			{
				//bool? completed = null;
				//var cts = new CancellationTokenSource();
				//try {
				//    var context = (string)state;
				//    await TaskEx.Run(() => WebSocket.SendAsync(context, (x) => {
				//        completed = x;
				//        cts.Cancel();
				//    })).ConfigureAwait(false);

				//    await TaskEx.Delay(1000 * 60, cts.Token);
				//    if (completed == false)
				//        throw new Exception(context);
				//} catch (Exception ex) {
				//    // Swallow exceptions on send
				//    Trace.TraceError("Error while sending: " + ex);
				//}				
				var contex = state as SendContext;
				var tcs = new TaskCompletionSource<MessageEventArgs>();
				EventHandler<MessageEventArgs> eventHandler = null;
				var cts = new CancellationTokenSource();
				var websocket = contex.Handler.WebSocket;
				cts.CancelAfter(TimeSpan.FromSeconds(60));
				eventHandler = (s, arg) =>
					{

						connection.Trace(TraceLevels.Messages, "OnMessage({0})", arg.Data);
						Newtonsoft.Json.Linq.JToken token = null;
						try
						{
							token = Newtonsoft.Json.Linq.JObject.Parse(arg.Data);
							
						}
						catch (Exception ex2)
						{
							connection.Trace(TraceLevels.Events, "Error ({0})", ex2.Message);
						}
						if (token != null && token["I"] != null)
						{
							websocket.OnMessage -= eventHandler;
							connection.OnReceived(token);
							tcs.SetResult(arg);
						}
					};
				websocket.OnMessage += eventHandler;

				TaskEx.Run(() => websocket.SendAsync(message, completed =>
					{
						if (completed == false)
							tcs.SetCanceled();
					}));
				return tcs.Task;

			}, new SendContext(this, buffer, WebSocketMessageType.Text, true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed"), Obsolete("Currently Not Supported", true)]
		public virtual Task SendAsync(ArraySegment<byte> message, WebSocketMessageType messageType, bool endOfMessage = true)
		{
			throw new NotSupportedException();
			//if (WebSocket.ReadyState != WebSocketState.Open) {
			//    return TaskAsyncHelper.Empty;
			//}

			//var sendContext = new SendContext(this, message, messageType, endOfMessage);

			//return _sendQueue.Enqueue(async state => {
			//    var context = (SendContext)state;

			//    if (context.Handler.WebSocket.ReadyState != WebSocketState.Open) {
			//        return;
			//    }

			//    try {
			//        await context.Handler.WebSocket.SendAsync(context.Message, context.MessageType, context.EndOfMessage, CancellationToken.None);
			//    } catch (Exception ex) {
			//        // Swallow exceptions on send
			//        Trace.TraceError("Error while sending: " + ex);
			//    }
			//},
			//sendContext);
		}

		public virtual Task CloseAsync()
		{
			if (IsClosedOrClosedSent(WebSocket))
			{
				return TaskAsyncHelper.Empty;
			}

			var closeContext = new CloseContext(this);

			return _sendQueue.Enqueue(async state =>
			{
				var context = (CloseContext)state;

				if (IsClosedOrClosedSent(context.Handler.WebSocket))
				{
					return;
				}

				try
				{
					WebSocket.CloseAsync(CloseStatusCode.Normal, "");

					await TaskEx.Run(() =>
					{
						while (WebSocket.ReadyState != WebSocketState.Closed) { }
					}, CancellationToken.None);
				}
				catch (Exception ex)
				{
					// Swallow exceptions on close
					Trace.TraceError("Error while closing the websocket: " + ex);
				}
			},
			closeContext);
		}

		public int? MaxIncomingMessageSize
		{
			get
			{
				return _maxIncomingMessageSize;
			}
		}

		internal WebSocket WebSocket { get; set; }

		public Exception Error { get; set; }

		public Task ProcessWebSocketRequestAsync(WebSocket webSocket, CancellationToken disconnectToken)
		{
			if (webSocket == null)
			{
				throw new ArgumentNullException("webSocket");
			}

			var receiveContext = new ReceiveContext(webSocket, disconnectToken, MaxIncomingMessageSize, _receiveLoopBufferSize);

			return ProcessWebSocketRequestAsync(webSocket, disconnectToken, state =>
			{
				var context = (ReceiveContext)state;

				return WebSocketMessageReader.ReadMessageAsync(context.WebSocket, context.BufferSize, context.MaxIncomingMessageSize, context.DisconnectToken);
			},
			receiveContext);
		}

		internal async Task ProcessWebSocketRequestAsync(WebSocket webSocket, CancellationToken disconnectToken, Func<object, Task<WebSocketMessage>> messageRetriever, object state)
		{
			bool closedReceived = false;

			try
			{
				// first, set primitives and initialize the object
				WebSocket = webSocket;
				OnOpen();

				// dispatch incoming messages
				while (!disconnectToken.IsCancellationRequested && !closedReceived)
				{
					WebSocketMessage incomingMessage = await messageRetriever(state);
					switch (incomingMessage.MessageType)
					{
						case WebSocketMessageType.Binary:
							OnMessage((byte[])incomingMessage.Data);
							break;

						case WebSocketMessageType.Text:
							OnMessage((string)incomingMessage.Data);
							break;

						default:
							closedReceived = true;

							// If we received an incoming CLOSE message, we'll queue a CLOSE frame to be sent.
							// We'll give the queued frame some amount of time to go out on the wire, and if a
							// timeout occurs we'll give up and abort the connection.
							await TaskEx.WhenAny(CloseAsync(), TaskEx.Delay(_closeTimeout));
							break;
					}
				}

			}
			catch (OperationCanceledException ex)
			{
				// ex.CancellationToken never has the token that was actually cancelled
				if (!disconnectToken.IsCancellationRequested)
				{
					Error = ex;
					OnError();
				}
			}
			catch (ObjectDisposedException)
			{
				// If the websocket was disposed while we were reading then noop
			}
			catch (Exception ex)
			{
				if (IsFatalException(ex))
				{
					Error = ex;
					OnError();
				}
			}

			try
			{
				if (WebSocket.ReadyState == WebSocketState.Closed/* ||
                    WebSocket.ReadyState == WebSocketState.Aborted*/)
				{
					// No-op if the socket is already closed or aborted
				}
				else
				{
					// Close the socket
					WebSocket.CloseAsync(CloseStatusCode.Normal, "");

					await TaskEx.Run(() =>
					{
						while (WebSocket.ReadyState != WebSocketState.Closed) { }
					}, CancellationToken.None);
				}
			}
			finally
			{
				OnClose();
			}
		}

		// returns true if this is a fatal exception (e.g. OnError should be called)
		private static bool IsFatalException(Exception ex)
		{
			// If this exception is due to the underlying TCP connection going away, treat as a normal close
			// rather than a fatal exception.
			COMException ce = ex as COMException;
			if (ce != null)
			{
				switch ((uint)ce.ErrorCode)
				{
					// These are the three error codes we've seen in testing which can be caused by the TCP connection going away unexpectedly.
					case 0x800703e3:
					case 0x800704cd:
					case 0x80070026:
						return false;
				}
			}

			// unknown exception; treat as fatal
			return true;
		}

		private static bool IsClosedOrClosedSent(WebSocket webSocket)
		{
			return webSocket.ReadyState == WebSocketState.Closed/* ||
                   webSocket.State == WebSocketState.CloseSent ||
                   webSocket.State == WebSocketState.Aborted*/;
		}

		private class CloseContext
		{
			public WebSocketHandler Handler;

			public CloseContext(WebSocketHandler webSocketHandler)
			{
				Handler = webSocketHandler;
			}
		}

		private class SendContext
		{
			public WebSocketHandler Handler;
			public byte[] Message;
			public WebSocketMessageType MessageType;
			public bool EndOfMessage;

			public SendContext(WebSocketHandler webSocketHandler, byte[] message, WebSocketMessageType messageType, bool endOfMessage)
			{
				Handler = webSocketHandler;
				Message = message;
				MessageType = messageType;
				EndOfMessage = endOfMessage;
			}
		}

		private class ReceiveContext
		{
			public WebSocket WebSocket;
			public CancellationToken DisconnectToken;
			public int? MaxIncomingMessageSize;
			public int BufferSize;

			public ReceiveContext(WebSocket webSocket, CancellationToken disconnectToken, int? maxIncomingMessageSize, int bufferSize)
			{
				WebSocket = webSocket;
				DisconnectToken = disconnectToken;
				MaxIncomingMessageSize = maxIncomingMessageSize;
				BufferSize = bufferSize;
			}
		}
	}
}