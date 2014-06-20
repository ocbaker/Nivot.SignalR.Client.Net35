using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.WebSockets
{
    public class DefaultWebSocketHandler : WebSocketHandler, IWebSocket
    {
        private readonly IWebSocket _webSocket;
        private volatile bool _closed;

        internal ArraySegment<byte> NextMessageToSend { get; private set; }

        public DefaultWebSocketHandler(int? maxIncomingMessageSize)
            : base(maxIncomingMessageSize) {
            _webSocket = this;

            _webSocket.OnClose = () => { };
            _webSocket.OnError = e => { };
            _webSocket.OnMessage = msg => { };
        }

        public override void OnClose() {
            _closed = true;

            _webSocket.OnClose();
        }

        public override void OnError() {
            _webSocket.OnError(Error);
        }

        public override void OnMessage(string message) {
            _webSocket.OnMessage(message);
        }

        Action<string> IWebSocket.OnMessage {
            get;
            set;
        }

        Action IWebSocket.OnClose {
            get;
            set;
        }

        Action<Exception> IWebSocket.OnError {
            get;
            set;
        }

        Task IWebSocket.Send(string value) {
            return Send(value);
        }

        public override Task Send(string message) {
            if (_closed) {
                return TaskAsyncHelper.Empty;
            }

            return base.Send(message);
        }

        public override Task CloseAsync() {
            if (_closed) {
                return TaskAsyncHelper.Empty;
            }

            return base.CloseAsync();
        }

        public Task SendChunk(ArraySegment<byte> message) {
            throw new NotSupportedException();
            //if (_closed) {
            //    return TaskAsyncHelper.Empty;
            //}

            //if (NextMessageToSend.Count == 0) {
            //    NextMessageToSend = message;
            //    return TaskAsyncHelper.Empty;
            //} else {
            //    ArraySegment<byte> messageToSend = NextMessageToSend;
            //    NextMessageToSend = message;
            //    return SendAsync(messageToSend, WebSocketMessageType.Text, endOfMessage: false);
            //}
        }

        public Task Flush() {
            throw new NotSupportedException();
            //if (_closed) {
            //    return TaskAsyncHelper.Empty;
            //}

            //var messageToSend = NextMessageToSend;
            //NextMessageToSend = new ArraySegment<byte>();

            //return SendAsync(messageToSend, WebSocketMessageType.Text, endOfMessage: true);
        }
    }
}
