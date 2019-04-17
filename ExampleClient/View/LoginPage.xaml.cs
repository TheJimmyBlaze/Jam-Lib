using JamLib.Client;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ExampleClient.View
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page, INotifyPropertyChanged
    {
        private const int CONNECT_TIMEOUT = 5000;

        private const string INVALID_CREDENTIALS = "Invalid username or password";
        private const string SERVER_UNREACHABLE = "Connection attempt timeout";

        #region Getters and Setters

        private bool awaitingLoginResponse = false;
        public bool AwaitingLoginResponse
        {
            get { return awaitingLoginResponse; }
            set
            {
                awaitingLoginResponse = value;
                NotifyPropertyChanged();
            }
        }

        public bool WorkInProgress
        {
            get { return AwaitingLoginResponse; }
            set { }
        }

        public bool LoginEnabled
        {
            get
            {
                return !WorkInProgress &&
                        address != string.Empty &&
                        port > 0 &&
                        Username != string.Empty &&
                        PasswordBox.Password != string.Empty;
            }
            set { }
        }

        private bool displayInvalidLoginMessage = false;
        public bool DisplayInvalidLoginMessage
        {
            get { return displayInvalidLoginMessage; }
            set
            {
                displayInvalidLoginMessage = value;
                NotifyPropertyChanged(nameof(DisplayInvalidLoginMessage));
            }
        }

        private string loginMessageText;
        public string LoginMessageText
        {
            get { return loginMessageText; }
            set
            {
                loginMessageText = value;
                NotifyPropertyChanged(nameof(LoginMessageText));

                if (loginMessageText == string.Empty)
                    DisplayInvalidLoginMessage = false;
                else
                    DisplayInvalidLoginMessage = true;
            }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                NotifyPropertyChanged(nameof(LoginEnabled));
            }
        }
        private int port;
        public string Port
        {
            get
            {
                if (port > 0)
                    return port.ToString();
                return string.Empty;
            }
            set
            {
                if (int.TryParse(value, out int intValue))
                    port = intValue;
                else
                    port = 0;
                NotifyPropertyChanged(nameof(LoginEnabled));
            }
        }
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyPropertyChanged(nameof(LoginEnabled));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public LoginPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            LoginMessageText = string.Empty;
            AwaitingLoginResponse = true;
            Cursor = Cursors.Wait;

            MainWindow main = App.Current.MainWindow as MainWindow;
            Task.Run(() =>
            {
                main.Client.Connect(address, port, CONNECT_TIMEOUT);
                if (!main.Client.IsConnected)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        LoginMessageText = SERVER_UNREACHABLE;
                        AwaitingLoginResponse = false;
                        if (!WorkInProgress)
                            Cursor = Cursors.Arrow;
                    });
                    return;
                }

                main.Client.Login(username, PasswordBox.Password);
            });
        }

        public void HandleLoginResponse(JamPacket packet)
        {
            LoginResponse response = new LoginResponse(packet.Data);

            if (response.Result == LoginResponse.LoginResult.Good)
            {
                //TODO: proceed with login.
            }
            else
            {
                LoginMessageText = INVALID_CREDENTIALS;
            }

            AwaitingLoginResponse = false;
            if (!WorkInProgress)
                Cursor = Cursors.Arrow;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void PortChanged(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            NotifyPropertyChanged(nameof(LoginEnabled));
        }

        private void ClearPassword()
        {
            PasswordBox.Password = string.Empty;
            PasswordBox.Focus();
        }

        public void NotifyPropertyChanged(string name = "")
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
