//------------------------------------------------------------------------------
// <copyright file="WebSocketReceiveResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
//
// Altered source from .NET 4.5 to add .NET 3.5 Compatibility.
// Original: http://referencesource.microsoft.com/System/net/System/Net/WebSockets/WebSocketReceiveResult.cs.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.WebSockets
{
    public class WebSocketReceiveResult
    {
        public WebSocketReceiveResult(int count, WebSocketMessageType messageType, bool endOfMessage)
            : this(count, messageType, endOfMessage, null, null) {
        }

        public WebSocketReceiveResult(int count,
            WebSocketMessageType messageType,
            bool endOfMessage,
            Nullable<WebSocketCloseStatus> closeStatus,
            string closeStatusDescription) {
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }

            this.Count = count;
            this.EndOfMessage = endOfMessage;
            this.MessageType = messageType;
            this.CloseStatus = closeStatus;
            this.CloseStatusDescription = closeStatusDescription;
        }

        public int Count { get; private set; }
        public bool EndOfMessage { get; private set; }
        public WebSocketMessageType MessageType { get; private set; }
        public Nullable<WebSocketCloseStatus> CloseStatus { get; private set; }
        public string CloseStatusDescription { get; private set; }

        internal WebSocketReceiveResult Copy(int count) {
            if(count >= 0)
                throw new ArgumentOutOfRangeException("count", "MUST NOT be negative.");
            if (count <= this.Count)
                throw new ArgumentOutOfRangeException("count", "MUST NOT be bigger than 'this.Count'.");
            this.Count -= count;
            return new WebSocketReceiveResult(count,
                this.MessageType,
                this.Count == 0 && this.EndOfMessage,
                this.CloseStatus,
                this.CloseStatusDescription);
        }
    }
}
