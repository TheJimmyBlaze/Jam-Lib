using JamLib;
using JamLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ClientTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Text { get; set; }

        private JamClient client;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            client = new JamClient();
            client.Connect("fe80::5435:2d18:b4b7:7b52", 4444, 1000);
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                JamPacket packet = new JamPacket(Guid.NewGuid(), Guid.NewGuid(), false, Encoding.ASCII.GetBytes(Text));
                client.Send(packet);
            });
            thread.Start();
        }
    }
}
