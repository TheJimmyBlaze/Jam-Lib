using ExampleClient.Domain;
using ExampleServer.Network.Data;
using JamLib.Database;
using JamLib.Packet;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExampleClient.View
{
    /// <summary>
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class MessagePage : Page, INotifyPropertyChanged
    {
        #region Getters and Setters
        private DisplayableAccount loggedInAccount;
        public DisplayableAccount LoggedInAccount
        {
            get { return loggedInAccount; }
            set
            {
                loggedInAccount = value;
                NotifyPropertyChanged(nameof(LoggedInAccount));
            }
        }

        private DisplayableAccount selectedAccount;
        public DisplayableAccount SelectedAccount
        {
            get { return selectedAccount; }
            set
            {
                selectedAccount = value;

                if (selectedAccount != null)
                {
                    foreach (DisplayableMessage message in selectedAccount.Messages)
                        message.Seen = true;
                }
                selectedAccount.NotifyPropertyChanged(nameof(UnseenMessages));

                NotifyPropertyChanged(nameof(SelectedAccount));
                NotifyPropertyChanged(nameof(CanSendMessage));
                NotifyPropertyChanged(nameof(UnseenMessages));
            }
        }

        private ObservableCollection<DisplayableAccount> accounts;
        public ObservableCollection<DisplayableAccount> Accounts
        {
            get
            {
                if (accounts == null)
                    accounts = new ObservableCollection<DisplayableAccount>();
                return accounts;
            }
            set { }
        }

        public string UnseenMessages
        {
            get
            {
                int unseenMessages = 0;
                foreach(DisplayableAccount account in Accounts)
                {
                    unseenMessages += account.UnseenMessages;
                }
                return string.Format("{0} new messages", unseenMessages);
            }
            set { }
        }

        public bool CanSendMessage
        {
            get
            {
                if (SelectedAccount != null && SelectedAccount.Online)
                    return true;
                return false;
            }
            set { }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public MessagePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow main = App.Current.MainWindow as MainWindow;

            LoggedInAccount = new DisplayableAccount(main.Client.Account, true);
            GetAccounts();
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            mainWindow.Client.Dispose();
        }

        public void GetAccounts()
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;

            GetAccountsRequest request = new GetAccountsRequest();
            JamPacket requestPacket = new JamPacket(Guid.Empty, Guid.Empty, GetAccountsRequest.DATA_TYPE, request.GetBytes());
            mainWindow.Client.Send(requestPacket);
        }

        public void HandleGetAccountsResponse(JamPacket packet)
        {
            if (packet.Header.DataType != GetAccountsResponse.DATA_TYPE)
                return;

            GetAccountsResponse response = new GetAccountsResponse(packet.Data); 
            foreach(Tuple<Account, bool> tuple in response.Accounts)
            {
                Account account = tuple.Item1;
                bool online = tuple.Item2;

                if (account.AccountID != LoggedInAccount.Account.AccountID)
                {
                    DisplayableAccount displayableAccount = new DisplayableAccount(account, online);
                    App.Current.Dispatcher.Invoke(() => Accounts.Add(displayableAccount));
                }
            }
        }

        public void HandleAccountOnlineStatusChangedImperative(JamPacket packet)
        {
            if (packet.Header.DataType != AccountOnlineStatusChangedImperative.DATA_TYPE)
                return;

            AccountOnlineStatusChangedImperative imperative = new AccountOnlineStatusChangedImperative(packet.Data);
            if (imperative.Account == null)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Guid selectedAccountID = Guid.Empty;
                if (selectedAccount != null)
                    selectedAccountID = SelectedAccount.Account.AccountID;

                DisplayableAccount account = Accounts.Single(x => x.Account.AccountID == imperative.Account.AccountID);
                Accounts.Remove(account);

                account.Online = imperative.Online;
                Accounts.Add(account);

                if (selectedAccountID == account.Account.AccountID)
                    SelectedAccount = account;

                NotifyPropertyChanged(nameof(CanSendMessage));
            });
        }

        public void NotifyPropertyChanged(string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SendMessage(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                MainWindow mainWindow = App.Current.MainWindow as MainWindow;

                TextBox messageBox = sender as TextBox;
                string message = messageBox.Text;

                messageBox.Clear();

                if (message != string.Empty)
                {
                    SendMessageImperative sendMessage = new SendMessageImperative() { Message = message };
                    JamPacket packet = new JamPacket(SelectedAccount.Account.AccountID, loggedInAccount.Account.AccountID, SendMessageImperative.DATA_TYPE, sendMessage.GetBytes());
                    mainWindow.Client.Send(packet);

                    DisplayableMessage sentMessage = new DisplayableMessage(DisplayableMessage.MessageType.Local, LoggedInAccount, packet.Header.SendTimeUtc, message);
                    SelectedAccount.AddMessage(sentMessage);
                }
            }
        }

        public void ReceiveMessage(JamPacket packet)
        {
            if (packet.Header.DataType != SendMessageImperative.DATA_TYPE)
                return;

            SendMessageImperative imperative = new SendMessageImperative(packet.Data);
            Guid senderID = packet.Header.Sender;
            DateTime sendTimeUtc = packet.Header.SendTimeUtc;

            App.Current.Dispatcher.Invoke(() =>
            {
                DisplayableAccount senderAccount = Accounts.SingleOrDefault(x => x.Account.AccountID == senderID);
                if (senderAccount == null)
                    return;

                DisplayableMessage receivedMessage = new DisplayableMessage(DisplayableMessage.MessageType.Remote, senderAccount, sendTimeUtc, imperative.Message);
                if (SelectedAccount != null && SelectedAccount.Account.AccountID == senderID)
                    receivedMessage.Seen = true;

                senderAccount.AddMessage(receivedMessage);

                NotifyPropertyChanged(nameof(UnseenMessages));
            });
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (e.ExtentHeightChange != 0)
                scrollViewer.ScrollToEnd();
        }

        private void MessageBoxEnabledOrDisabled(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.IsEnabled)
                textBox.Focus();
        }
    }
}
