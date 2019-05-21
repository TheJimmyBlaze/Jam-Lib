using ExampleClient.View;
using JamLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExampleClient.Network
{
    internal static class ClientEventHandler
    {
        internal static void OnClientDisposed(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;

                LoginPage loginPage = new LoginPage();
                loginPage.NotifyDisconnect();

                main.Navigate(loginPage);
                main.Cursor = Cursors.Arrow;
            });
        }

        internal static void OnMessageReceived(object sender, JamClient.MessageReceivedEventArgs e)
        {
            ClientInterpreter.Interpret(e.Packet);
        }
    }
}
