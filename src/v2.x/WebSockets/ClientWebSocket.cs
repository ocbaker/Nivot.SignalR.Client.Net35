using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.WebSockets
{
    public class ClientWebSocket : WebSocket
    {
        CancellationToken _cancellationToken;

        public ClientWebSocket(string url, CancellationToken token, params string[] protocols) : base(url, protocols) {
            _cancellationToken = token;
        }
    }

    //public interface IClientWebSocket
    //{

    //    // Summary:
    //    //     Indicates the reason why the close handshake was initiated.
    //    //
    //    // Returns:
    //    //     Returns System.Net.WebSockets.WebSocketCloseStatus.
    //    WebSocketCloseStatus? CloseStatus { get; }
    //    //
    //    //
    //    // Returns:
    //    //     Returns System.String.
    //    string CloseStatusDescription { get; }
    //    //
    //    // Summary:
    //    //     Represents the WebSocket client options.
    //    //
    //    // Returns:
    //    //     Returns System.Net.WebSockets.ClientWebSocketOptions.
    //    ClientWebSocketOptions Options { get; }
    //    //
    //    // Summary:
    //    //     Represents the client WebSocket state.
    //    //
    //    // Returns:
    //    //     Returns System.Net.WebSockets.WebSocketState.
    //    WebSocketState State { get; }
    //    //
    //    //
    //    // Returns:
    //    //     Returns System.String.
    //    string SubProtocol { get; }

    //    // Summary:
    //    //     Aborts the connection and cancels any pending IO operations.
    //    void Abort();
    //    //
    //    //
    //    // Parameters:
    //    //   closeStatus:
    //    //     Represents WebSocket close codes.
    //    //
    //    //   statusDescription:
    //    //     Friendly description of the close status.
    //    //
    //    // Returns:
    //    //     Returns System.Threading.Tasks.Task.
    //    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    //    //
    //    //
    //    // Returns:
    //    //     Returns System.Threading.Tasks.Task.
    //    Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    //    //
    //    // Summary:
    //    //     Begins an asynchronous connection request.
    //    //
    //    // Returns:
    //    //     Returns System.Threading.Tasks.Task.
    //    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    //    void Dispose();
    //    //
    //    // Summary:
    //    //     Begins an asynchronous receive request.
    //    //
    //    // Parameters:
    //    //   buffer:
    //    //     The buffer to receive the response.
    //    //
    //    // Returns:
    //    //     Returns System.Threading.Tasks.Task<TResult>.
    //    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    //    //
    //    // Summary:
    //    //     Begins an asynchronous send request.
    //    //
    //    // Parameters:
    //    //   buffer:
    //    //     The buffer containing the message to be sent.
    //    //
    //    //   messageType:
    //    //     Specifies whether the buffer is clear text or in a binary format.
    //    //
    //    //   endOfMessage:
    //    //     Specifies whether this is the final asynchronous send. Set to true if this
    //    //     is the final send; false otherwise.
    //    //
    //    // Returns:
    //    //     Returns System.Threading.Tasks.Task.
    //    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    //}
}
