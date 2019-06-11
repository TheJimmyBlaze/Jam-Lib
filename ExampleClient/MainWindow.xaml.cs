using ExampleClient.Network;
using ExampleClient.View;
using JamLib.Client;
using JamLib.Domain.Serialization;
using JamLib.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExampleClient
{
    public partial class MainWindow : Window
    {
        public JamClient Client { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ResetClient();

            LoginPage loginPage = new LoginPage();
            Navigate(loginPage);
        }

        public void ResetClient()
        {
            Client = new JamClient(ExampleServer.Program.APP_SIGNITURE, new Utf8JsonSerializer());
            Client.DisposedEvent += ClientEventHandler.OnClientDisposed;
            Client.MessageReceivedEvent += ClientEventHandler.OnMessageReceived;
            Client.LoginResultEvent += ClientEventHandler.OnLoginResult;
        }

        public void Navigate(Page page)
        {
            ViewFrame.Navigate(page);
        }
    }
}
