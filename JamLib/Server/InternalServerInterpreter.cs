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
                    serverConnection.RespondToLogin(packet);
                    break;
                case PingRequest.DATA_TYPE:
                    serverConnection.RespondToPing(packet);
                    break;
                case RegisterDataTypesRequest.DATA_TYPE:
                    serverConnection.RespondToDataTypeRegistration(packet);
                    break;
                default:
                    serverConnection.Server.OnMessageReceived(new JamServer.MessageReceivedEventArgs() { ServerConnection = serverConnection, Packet = packet });
                    break;
            }
        }
    }
}
