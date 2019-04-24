using JamLib.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Server
{
    internal static class JamPacketRouter
    {
        internal static void Route(JamServerConnection serverConnection, JamPacket packet)
        {
            JamServer server = serverConnection.Server;
            Guid recipient = packet.Header.Receipient;

            if (recipient == Guid.Empty)
            {
                InternalServerInterpreter.Interpret(serverConnection, packet);
                return;
            }

            JamServerConnection recipientConnection = server.GetConnection(recipient);
            if (recipientConnection == null)
            {
                server.OnUndelieveredMessageReceived(new JamServer.MessageReceivedEventArgs() { ServerConnection = serverConnection, Packet = packet });
            }
            else
                recipientConnection.Send(packet);   
        }
    }
}
