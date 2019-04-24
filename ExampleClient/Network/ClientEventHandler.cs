using ExampleClient.View;
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
        internal static void OnClientDisposed(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow.Navigate(new LoginPage());
            });
        }

        internal static void OnMessageReceived(object sender, JamClient.MessageReceivedEventArgs e)
        {
            ChatClientInterpreter.Interpret(e.Packet);
        }
    }
}
