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

            IJamPacketInterpreter interpreter = new ChatClientInterpreter();
            Client = new JamClient(interpreter);

            //LoginPage loginPage = new LoginPage();
            MessagePage test = new MessagePage();
            Navigate(test);
        }

        public void Navigate(Page page)
        {
            ViewFrame.Navigate(page);
        }
    }
}
