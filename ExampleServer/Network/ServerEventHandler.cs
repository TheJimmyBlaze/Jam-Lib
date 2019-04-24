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
        internal static void OnMessageReceived(object sender, JamServer.MessageReceivedEventArgs args)
        {
            ServerInterpreter.Interpret(args.ServerConnection, args.Packet);
        }

        internal static void OnUndeliveredMessageReceived(object sender, JamServer.MessageReceivedEventArgs args)
        {
            Console.WriteLine("Received undeliverable message. Intended recipient: {0}, data type: {1}", args.Packet.Header.Receipient, args.Packet.Header.DataType);
        }

        internal static void OnClientConnected(object sender, JamServer.ConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} connected.", args.Client.Client.RemoteEndPoint);
        }

        internal static void OnClientDisconnect(object sender, JamServer.ConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} disconnected.", args.Client.Client.RemoteEndPoint);
        }

        internal static void OnClientIdentified(object sender, JamServer.IdentifiedConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} identified as: {1} - {2}", args.Client.Client.RemoteEndPoint, args.Account.AccountID, args.Account.Username);
        }

        internal static void OnClientInvalidUsername(object sender, JamServer.ConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} provided an invalid username.", args.Client.Client.RemoteEndPoint);
        }

        internal static void OnClientInvalidPassword(object sender, JamServer.ConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} provided an invalid password.", args.Client.Client.RemoteEndPoint);
        }

        internal static void OnClientConnectedElsewhere(object sender, JamServer.IdentifiedConnectionEventArgs args)
        {
            Console.WriteLine("Client: {0} has connected elsewhere, closing existing connection...", args.Client.Client.RemoteEndPoint);
        }
    }
}
