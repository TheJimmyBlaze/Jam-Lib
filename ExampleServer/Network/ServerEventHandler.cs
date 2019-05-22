using ExampleServer.Network.Data;
using JamLib.Packet;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network
{
    internal static class ServerEventHandler
    {
        internal static void OnMessageReceived(object sender, JamServer.MessageReceivedEventArgs e)
        {
            ServerInterpreter.Interpret(e.ServerConnection, e.Packet);
        }

        internal static void OnUndeliveredMessageReceived(object sender, JamServer.MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received undeliverable message. Intended recipient: {0}, data type: {1}.", e.Packet.Header.Receipient, e.Packet.Header.DataType);
        }

        internal static void OnClientConnected(object sender, JamServer.ConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} connected.", e.RemoteEndPoint);
        }

        internal static void OnClientDisconnect(object sender, JamServer.IdentifiedConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} disconnected.", e.RemoteEndPoint);

            AccountOnlineStatusChangedImperative imperative = new AccountOnlineStatusChangedImperative(e.Account, false, e.ServerConnection.Serializer);

            JamPacket imperativePacket = new JamPacket(Guid.Empty, Guid.Empty, AccountOnlineStatusChangedImperative.DATA_TYPE, imperative.GetBytes());
            foreach(JamServerConnection connection in e.ServerConnection.Server.GetAllConnections()){
                connection.Send(imperativePacket);
            }
        }

        internal static void OnClientIdentified(object sender, JamServer.IdentifiedConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} identified as: {1} - {2}.", e.RemoteEndPoint, e.Account.AccountID, e.Account.Username);

            AccountOnlineStatusChangedImperative imperative = new AccountOnlineStatusChangedImperative(e.Account, true, e.ServerConnection.Serializer);

            JamPacket imperativePacket = new JamPacket(Guid.Empty, Guid.Empty, AccountOnlineStatusChangedImperative.DATA_TYPE, imperative.GetBytes());
            foreach(JamServerConnection connection in e.ServerConnection.Server.GetAllConnections())
            {
                connection.Send(imperativePacket);
            }
        }

        internal static void OnClientInvalidUsername(object sender, JamServer.ConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} provided an invalid username.", e.RemoteEndPoint);
        }

        internal static void OnClientInvalidPassword(object sender, JamServer.ConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} provided an invalid password.", e.RemoteEndPoint);
        }

        internal static void OnClientConnectedElsewhere(object sender, JamServer.IdentifiedConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} has connected elsewhere, closing existing connection...", e.RemoteEndPoint);
        }

        internal static void OnClientOfflineAppRequest(object sender, JamServer.IdentifiedConnectionEventArgs e)
        {
            Console.WriteLine("Client: {0} requested access to an app not currently running against this server.", e.RemoteEndPoint);
        }

        internal static void OnDisposed(object sender, EventArgs e)
        {
            Console.WriteLine("Server disposed.");
            Program.Exit();
        }
    }
}
