using ExampleClient.Network;
using ExampleClient.View;
using JamLib.Client;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public JamClient Client { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            Client = new JamClient();
            Client.MessageReceivedEvent += ClientEventHandler.OnMessageReceived;

            LoginPage loginPage = new LoginPage();
            Navigate(loginPage);
        }

        public void Navigate(Page page)
        {
            ViewFrame.Navigate(page);
        }
    }
}
