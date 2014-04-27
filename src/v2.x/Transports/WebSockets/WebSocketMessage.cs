using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.SignalR.Client.Transports.WebSockets
{
    internal sealed class WebSocketMessage
    {
        public static readonly WebSocketMessage EmptyTextMessage = new WebSocketMessage(String.Empty, WebSocketMessageType.Text);
        public static readonly WebSocketMessage EmptyBinaryMessage = new WebSocketMessage(new byte[0], WebSocketMessageType.Binary);
        public static readonly WebSocketMessage CloseMessage = new WebSocketMessage(null, WebSocketMessageType.Close);

        public readonly object Data;
        public readonly WebSocketMessageType MessageType;

        public WebSocketMessage(object data, WebSocketMessageType messageType) {
            Data = data;
            MessageType = messageType;
        }
    }
}
