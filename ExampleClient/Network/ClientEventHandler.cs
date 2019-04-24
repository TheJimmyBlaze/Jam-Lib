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
                MainWindow main = App.Current.MainWindow as MainWindow;
                if (!(main.ViewFrame.Content is LoginPage))
                    main.Navigate(new LoginPage());
            });
        }

        internal static void OnMessageReceived(object sender, JamClient.MessageReceivedEventArgs e)
        {
            ClientInterpreter.Interpret(e.Packet);
        }
    }
}
