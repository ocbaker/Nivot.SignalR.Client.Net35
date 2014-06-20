using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.WebSockets;
using NetworkCredential = WebSocketSharp.Net.NetworkCredential;

namespace Microsoft.AspNet.SignalR.Client.Transports.WebSockets
{

    public class NetworkCredentialMap : NetworkCredential, ICredentials
    {
        public NetworkCredentialMap(string username, string password) : base(username, password) {}
        public NetworkCredentialMap(string username, string password, string domain, params string[] roles) : base(username, password, domain, roles) {}
        public System.Net.NetworkCredential GetCredential(Uri uri, string authType) {
            throw new NotImplementedException();
        }
    }

    internal class WebSocketWrapperRequest : IRequest
    {
        private readonly ClientWebSocket _clientWebSocket;
        private IConnection _connection;

        public WebSocketWrapperRequest(ClientWebSocket clientWebSocket, IConnection connection) {
            _clientWebSocket = clientWebSocket;
            _connection = connection;
            PrepareRequest();
        }

        public string UserAgent {
            get {
                return null;
            }
            set {

            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public ICredentials Credentials {
            get {
                return (NetworkCredentialMap)_clientWebSocket.Credentials;
            }
            set {
                _clientWebSocket.Credentials = (NetworkCredential)value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public CookieContainer CookieContainer {
            get {
                throw new NotSupportedException();
                //return _clientWebSocket.Cookies;
            }
            set {
                throw new NotSupportedException();
                //_clientWebSocket.Options.Cookies = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public IWebProxy Proxy {
            get {
                throw new NotSupportedException();
                //return _clientWebSocket.Proxy;
            }
            set {
                throw new NotSupportedException();
                //_clientWebSocket.Options.Proxy = value;
            }
        }

        public string Accept {
            get {
                return null;
            }
            set {

            }
        }

        public void SetRequestHeaders(IDictionary<string, string> headers) {
            if (headers == null) {
                throw new ArgumentNullException("headers");
            }

            foreach (KeyValuePair<string, string> headerEntry in headers) {
                throw new NotSupportedException();
                //_clientWebSocket.SetRequestHeader(headerEntry.Key, headerEntry.Value);
            }
        }

        public void AddClientCerts(X509CertificateCollection certificates) {
            if (certificates == null) {
                throw new ArgumentNullException("certificates");
            }
            throw new NotSupportedException();
            //_clientWebSocket.ClientCertificates = certificates;
        }

        public void Abort() {

        }

        /// <summary>
        /// Adds certificates, credentials, proxies and cookies to the request
        /// </summary>
        private void PrepareRequest() {
            if (_connection.Certificates != null) {
                AddClientCerts(_connection.Certificates);
            }

            if (_connection.CookieContainer != null) {
                CookieContainer = _connection.CookieContainer;
            }

            if (_connection.Credentials != null) {
                Credentials = _connection.Credentials;
            }

            if (_connection.Proxy != null) {
                Proxy = _connection.Proxy;
            }
        }
    }
}
