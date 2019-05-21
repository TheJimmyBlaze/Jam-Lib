using JamLib.Database;
using JamLib.Domain.Cryptography;
using JamLib.Domain.Serialization;
using JamLib.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamServer: IDisposable
    {
        #region Event Handlers
        public class MessageReceivedEventArgs : EventArgs
        {
            public JamServerConnection ServerConnection { get; set; }
            public JamPacket Packet { get; set; }
        }

        public class ConnectionEventArgs: EventArgs
        {
            public JamServerConnection ServerConnection { get; set; }
            public TcpClient Client { get; set; }
        }

        public class IdentifiedConnectionEventArgs: EventArgs
        {
            public JamServerConnection ServerConnection { get; set; }
            public TcpClient Client { get; set; }
            public Account Account { get; set; }
        }

        public EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public EventHandler<MessageReceivedEventArgs> UndeliveredMessageReceivedEvent;

        public EventHandler<ConnectionEventArgs> ClientConnectedEvent;
        public EventHandler<IdentifiedConnectionEventArgs> ClientDisconnectedEvent;

        public EventHandler<IdentifiedConnectionEventArgs> ClientIdentifiedEvent;
        public EventHandler<ConnectionEventArgs> ClientInvalidUsernameEvent;
        public EventHandler<ConnectionEventArgs> ClientInvalidPasswordEvent;

        public EventHandler<IdentifiedConnectionEventArgs> ClientConnectedElsewhereEvent;

        public EventHandler DisposedEvent;

        public void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceivedEvent?.Invoke(this, e);
        }

        public void OnUndelieveredMessageReceived(MessageReceivedEventArgs e)
        {
            UndeliveredMessageReceivedEvent?.Invoke(this, e);
        }

        public void OnClientConnected(ConnectionEventArgs e)
        {
            ClientConnectedEvent?.Invoke(this, e);
        }

        public void OnClientDisconnected(IdentifiedConnectionEventArgs e)
        {
            ClientDisconnectedEvent?.Invoke(this, e);
        }

        public void OnClientIdentified(IdentifiedConnectionEventArgs e)
        {
            ClientIdentifiedEvent?.Invoke(this, e);
        }

        public void OnClientInvalidUsername(ConnectionEventArgs e)
        {
            ClientInvalidUsernameEvent?.Invoke(this, e);
        }

        public void OnClientInvalidPassword(ConnectionEventArgs e)
        {
            ClientInvalidPasswordEvent?.Invoke(this, e);
        }

        public void OnClientConnectedElsewhere(IdentifiedConnectionEventArgs e)
        {
            ClientConnectedElsewhereEvent?.Invoke(this, e);
        }

        public void OnDisposed(EventArgs e)
        {
            DisposedEvent?.Invoke(this, e);
        }
        #endregion

        private struct ConnectState
        {
            public TcpClient Client { get; set; }
            public SslStream Stream { get; set; }
        }

        private readonly List<JamServerConnection> connections = new List<JamServerConnection>();
        private readonly AutoResetEvent acceptCompleted = new AutoResetEvent(false);

        private X509Certificate serverCertificate;
        public readonly IHashFactory HashFactory;

        private bool alive;

        public readonly ISerializer Serializer;

        public JamServer(IHashFactory hashFactory, ISerializer serializer)
        {
            HashFactory = hashFactory;
            Serializer = serializer;
        }

        public List<JamServerConnection> GetAllConnections()
        {
            return connections;
        }

        public JamServerConnection GetConnection(Guid accountID)
        {
            return connections.SingleOrDefault(x => x.Account.AccountID == accountID);
        }

        public void AddConnection(JamServerConnection connection)
        {
            connections.Add(connection);
        }

        public void DeleteConnection(Guid accountID)
        {
            connections.Remove(GetConnection(accountID));
        }

        public void Start(int port, string certificate, string certificatePassword)
        {
            serverCertificate = new X509Certificate2(certificate, certificatePassword, X509KeyStorageFlags.Exportable);
            alive = true;

            Task.Run(() => { Listen(port); });
        }

        public void Dispose()
        {
            while (connections.Count > 0)
                connections[0].Dispose();

            alive = false;
            OnDisposed(null);
        }

        private void Listen(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (alive)
            {
                TcpClient client = listener.AcceptTcpClient();
                SslStream stream = new SslStream(client.GetStream(), false);

                ConnectState state = new ConnectState()
                {
                    Client = client,
                    Stream = stream
                };

                stream.BeginAuthenticateAsServer(serverCertificate, AcceptCallback, state);
                acceptCompleted.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ConnectState state = (ConnectState)result.AsyncState;
            if (state.Stream.CanRead)
                state.Stream.EndAuthenticateAsServer(result);
            else
                return;

            new JamServerConnection(state.Client, state.Stream, this);

            acceptCompleted.Set();
        }
    }
}
