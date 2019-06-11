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
                main.ResetClient();

                if (!(main.ViewFrame.Content is LoginPage))
                {
                    LoginPage loginPage = new LoginPage();
                    loginPage.NotifyDisconnect();

                    main.Navigate(loginPage);
                }

                main.Cursor = Cursors.Arrow;
            });
        }

        internal static void OnMessageReceived(object sender, JamClient.MessageReceivedEventArgs e)
        {
            ClientInterpreter.Interpret(e.Packet);
        }

        internal static void OnLoginResult(object sender, JamClient.LoginResultEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;

                if (main.ViewFrame.Content is LoginPage page)
                {
                    page.HandleLoginResult(e.Result);
                }
            });
        }
    }
}
