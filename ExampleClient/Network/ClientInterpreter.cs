using ExampleClient.View;
using ExampleServer.Network.Data;
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
    internal static class ClientInterpreter
    {
        internal static void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case RegisterAccountResponse.DATA_TYPE:
                    HandleAccountRegistrationResponse(packet);
                    break;
                case GetAccountsResponse.DATA_TYPE:
                    HandleGetAccountsResponse(packet);
                    break;
                case AccountOnlineStatusChangedImperative.DATA_TYPE:
                    HandleOnlineStatusChangedImperative(packet);
                    break;
                case SendMessageImperative.DATA_TYPE:
                    HandleSendMessageImperative(packet);
                    break;
            }
        }

        private static void HandleAccountRegistrationResponse(JamPacket packet)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main.ViewFrame.Content is LoginPage page)
                {
                    Task.Run(() => page.HandleRegistrationResponse(packet));
                }
            });
        }

        private static void HandleGetAccountsResponse(JamPacket packet)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main.ViewFrame.Content is MessagePage page)
                {
                    Task.Run(() => page.HandleGetAccountsResponse(packet));
                }
            });
        }

        private static void HandleOnlineStatusChangedImperative(JamPacket packet)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main.ViewFrame.Content is MessagePage page)
                {
                    Task.Run(() => page.HandleAccountOnlineStatusChangedImperative(packet));
                }
            });
        }

        private static void HandleSendMessageImperative(JamPacket packet)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (main.ViewFrame.Content is MessagePage page)
                {
                    Task.Run(() => page.ReceiveMessage(packet));
                }
            });
        }
    }
}
