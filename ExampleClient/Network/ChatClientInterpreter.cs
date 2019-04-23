using ExampleClient.View;
using JamLib.Client;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleClient.Network
{
    internal static class ChatClientInterpreter
    {
        internal static void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case LoginResponse.DATA_TYPE:
                    HandleLoginResponse(packet);
                    break;
            }
        }

        private static void HandleLoginResponse(JamPacket packet)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main.ViewFrame.Content is LoginPage page)
                {
                    page.HandleLoginResponse(packet);
                }
            });
        }
    }
}
