using JamLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleClient.Network
{
    internal static class ClientEventHandler
    {

        internal static void OnMessageReceived(object sender, JamClient.MessageReceivedEventArgs args)
        {
            ChatClientInterpreter.Interpret(args.Packet);
        }
    }
}
