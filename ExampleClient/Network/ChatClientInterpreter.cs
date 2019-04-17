using ExampleClient.View;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleClient.Network
{
    public class ChatClientInterpreter : IJamPacketInterpreter
    {
        public void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case LoginResponse.DATA_TYPE:
                    HandleLoginResponse(packet);
                    break;
            }
        }

        private void HandleLoginResponse(JamPacket packet)
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
