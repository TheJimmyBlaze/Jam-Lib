using JamLib.Packet;
using JamLib.Packet.Data;

namespace JamLib.Server
{
    internal static class InternalServerInterpreter
    {
        internal static void Interpret(JamServerConnection serverConnection, JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case LoginRequest.DATA_TYPE:
                    serverConnection.Login(packet);
                    break;
                case PingRequest.DATA_TYPE:
                    serverConnection.Ping(packet);
                    break;
                default:
                    serverConnection.Server.OnMessageReceived(new JamServer.MessageReceivedEventArgs() { ServerConnection = serverConnection, Packet = packet });
                    break;
            }
        }
    }
}
