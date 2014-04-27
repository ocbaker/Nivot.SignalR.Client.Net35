using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.WebSockets
{
    //// Summary:
    ////     An instance of this class represents the result of performing a single ReceiveAsync
    ////     operation on a WebSocket.
    //public class WebSocketReceiveResult
    //{
    //    // Summary:
    //    //     Creates an instance of the System.Net.WebSockets.WebSocketReceiveResult class.
    //    //
    //    // Parameters:
    //    //   count:
    //    //     The number of bytes received.
    //    //
    //    //   messageType:
    //    //     The type of message that was received.
    //    //
    //    //   endOfMessage:
    //    //     Indicates whether this is the final message.
    //    public WebSocketReceiveResult(int count, WebSocketMessageType messageType, bool endOfMessage);
    //    //
    //    // Summary:
    //    //     Creates an instance of the System.Net.WebSockets.WebSocketReceiveResult class.
    //    //
    //    // Parameters:
    //    //   count:
    //    //     The number of bytes received.
    //    //
    //    //   messageType:
    //    //     The type of message that was received.
    //    //
    //    //   endOfMessage:
    //    //     Indicates whether this is the final message.
    //    //
    //    //   closeStatus:
    //    //     Indicates the System.Net.WebSockets.WebSocketCloseStatus of the connection.
    //    //
    //    //   closeStatusDescription:
    //    //     The description of closeStatus.
    //    public WebSocketReceiveResult(int count, WebSocketMessageType messageType, bool endOfMessage, WebSocketCloseStatus? closeStatus, string closeStatusDescription);

    //    // Summary:
    //    //     Indicates the reason why the remote endpoint initiated the close handshake.
    //    //
    //    // Returns:
    //    //     Returns System.Net.WebSockets.WebSocketCloseStatus.
    //    public WebSocketCloseStatus? CloseStatus { get; }
    //    //
    //    // Summary:
    //    //     Returns the optional description that describes why the close handshake has
    //    //     been initiated by the remote endpoint.
    //    //
    //    // Returns:
    //    //     Returns System.String.
    //    public string CloseStatusDescription { get; }
    //    //
    //    // Summary:
    //    //     Indicates the number of bytes that the WebSocket received.
    //    //
    //    // Returns:
    //    //     Returns System.Int32.
    //    public int Count { get; }
    //    //
    //    // Summary:
    //    //     Indicates whether the message has been received completely.
    //    //
    //    // Returns:
    //    //     Returns System.Boolean.
    //    public bool EndOfMessage { get; }
    //    //
    //    // Summary:
    //    //     Indicates whether the current message is a UTF-8 message or a binary message.
    //    //
    //    // Returns:
    //    //     Returns System.Net.WebSockets.WebSocketMessageType.
    //    public WebSocketMessageType MessageType { get; }
    //}
}
