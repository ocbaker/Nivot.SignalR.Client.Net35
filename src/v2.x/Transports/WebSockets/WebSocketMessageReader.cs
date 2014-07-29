using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.WebSockets;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.Transports.WebSockets
{
    internal static class WebSocketMessageReader
    {
        static async Task<WebSocketReceiveResult> WSRecieveAsync(WebSocket webSocket, CancellationToken disconnectToken) {
            WebSocketReceiveResult result = null;
            Action removeEvent = null;
            bool cancelled = true;
            EventHandler<MessageEventArgs> webSocketOnOnMessage = (sender, args) => {
                result = new WebSocketReceiveResult(args.Data.Count(), args.Type.GetWSMessageType(), true) { Message = args.Data };
                removeEvent();
            };
            removeEvent = () => webSocket.OnMessage -= webSocketOnOnMessage;
            webSocket.OnMessage += webSocketOnOnMessage;

            await TaskEx.Run(() => {
                while (result == null && !disconnectToken.IsCancellationRequested) {
                    Thread.Sleep(10);
                }
                if (disconnectToken.IsCancellationRequested) {
                    cancelled = true;
                    return;
                }
                cancelled = false;
            }, disconnectToken);
            if (cancelled)
                removeEvent();
            return result;
        }

        public static async Task<WebSocketMessage> ReadMessageAsync(WebSocket webSocket, int bufferSize,
            int? maxMessageSize, CancellationToken disconnectToken) {
            WebSocketReceiveResult receiveResult = await WSRecieveAsync(webSocket, disconnectToken).ConfigureAwait(false);
            if(receiveResult.MessageType == WebSocketMessageType.Close)
                return WebSocketMessage.CloseMessage;
            return new WebSocketMessage(receiveResult.Message, receiveResult.MessageType);
        }
    }
}
