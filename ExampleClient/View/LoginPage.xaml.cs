using ExampleServer.Network.Data;
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

        private const string SERVER_UNREACHABLE = "Connection attempt timeout";
        private const string SERVER_DISCONNECT = "Disconnected from server";
        private const string APP_OFFLINE = "Service unavailable";

        private const string INVALID_CREDENTIALS = "Invalid username or password";
        private const string USERNAME_IN_USE = "Username already in use";

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

        private bool displayLoginMessage = false;
        public bool DisplayLoginMessage
        {
            get { return displayLoginMessage; }
            set
            {
                displayLoginMessage = value;
                NotifyPropertyChanged(nameof(DisplayLoginMessage));
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
                    DisplayLoginMessage = false;
                else
                    DisplayLoginMessage = true;
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

            LoadLoginSettings();
            FocusFirstEmptyLoginCredential();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            SaveLoginSettings();
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
                        main.Client.Dispose();
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

        private void Register(object sender, RoutedEventArgs e)
        {
            SaveLoginSettings();
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
                        main.Client.Dispose();
                        loginMessageText = SERVER_UNREACHABLE;
                        AwaitingLoginResponse = false;
                        if (!WorkInProgress)
                            Cursor = Cursors.Arrow;
                    });
                    return;
                }

                RegisterAccountRequest request = new RegisterAccountRequest(username, PasswordBox.Password, main.Client.Serializer);
                JamPacket requestPacket = new JamPacket(Guid.Empty, Guid.Empty, RegisterAccountRequest.DATA_TYPE, request.GetBytes());
                main.Client.Send(requestPacket);
            });
        }

        public void HandleLoginResponse(JamPacket packet)
        {
            if (packet.Header.DataType != LoginResponse.DATA_TYPE)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                LoginResponse response = new LoginResponse(packet.Data, main.Client.Serializer);

                if (response.Result == LoginResponse.LoginResult.Good)
                {
                    main.Client.Account = response.Account;
                    MessagePage messagePage = new MessagePage();
                    main.Navigate(messagePage);
                }
                else
                {
                    main.Client.Dispose();
                    ClearPassword();

                    if (response.Result == LoginResponse.LoginResult.AppOffline)
                        LoginMessageText = APP_OFFLINE;
                    else
                        LoginMessageText = INVALID_CREDENTIALS;
                }

                AwaitingLoginResponse = false;
                if (!WorkInProgress)
                    Cursor = Cursors.Arrow;
            });
        }

        public void HandleRegistrationResponse(JamPacket packet)
        {
            if (packet.Header.DataType != RegisterAccountResponse.DATA_TYPE)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow main = App.Current.MainWindow as MainWindow;
                RegisterAccountResponse response = new RegisterAccountResponse(packet.Data, main.Client.Serializer);

                if (response.Result == RegisterAccountResponse.AccountRegistrationResult.Good)
                {
                    main.Client.Dispose();
                    Login(null, null);
                    return;
                }
                else
                {
                    main.Client.Dispose();
                    LoginMessageText = USERNAME_IN_USE;
                    ClearPassword();
                }

                AwaitingLoginResponse = false;
                if (!WorkInProgress)
                    Cursor = Cursors.Arrow;
            });
        }

        private void LoadLoginSettings()
        {
            Address = Properties.Settings.Default.IPAddress;
            Port = Properties.Settings.Default.Port.ToString();
            Username = Properties.Settings.Default.Username;
        }

        private void SaveLoginSettings()
        {
            Properties.Settings.Default.IPAddress = Address;
            Properties.Settings.Default.Port = port;
            Properties.Settings.Default.Username = Username;

            Properties.Settings.Default.Save();
        }

        private void FocusFirstEmptyLoginCredential()
        {
            if (Address == string.Empty)
                AddressTextBox.Focus();
            else if (Port == string.Empty)
                PortTextBox.Focus();
            else if (Username == string.Empty)
                UsernameTextBox.Focus();
            else
                PasswordBox.Focus();
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
            PasswordBox.IsEnabled = true;
            PasswordBox.Focus();
        }

        public void NotifyDisconnect()
        {
            LoginMessageText = SERVER_DISCONNECT;
        }

        public void NotifyPropertyChanged(string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
