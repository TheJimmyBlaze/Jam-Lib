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

                //TODO: Remove this testing code.
                MessageSession testSession = new MessageSession(new DisplayableMessage(DisplayableMessage.MessageType.System, new DisplayableAccount(selectedAccount.Account, true), DateTime.UtcNow,
                    string.Format("Begining of message history with {0} for this session.", selectedAccount.Account.Username)));
                testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Remote, new DisplayableAccount(selectedAccount.Account, true), DateTime.UtcNow,
                    "This is an example test message from a remote account. This message is very long, and artificially lengthened. I want this message to span multiple lines to test out the text wrapping functionality."));
                testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Remote, new DisplayableAccount(selectedAccount.Account, true), DateTime.UtcNow,
                    "Here is another example message. This one is shorter."));
                testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Local, LoggedInAccount, DateTime.UtcNow,
                    "This is a similar example message, this time from the local account."));
                selectedAccount.MessageSession = testSession;

                NotifyPropertyChanged(nameof(SelectedAccount));
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

        private int unreadMessages = 107;
        public int UnreadMessages
        {
            get { return unreadMessages; }
            set
            {
                unreadMessages = value;
                NotifyPropertyChanged(nameof(UnreadMessages));
                NotifyPropertyChanged(nameof(UnreadMessagesText));
            }
        }

        public string UnreadMessagesText
        {
            get { return string.Format("{0} unread messages", UnreadMessages); }
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
                DisplayableAccount account = Accounts.Single(x => x.Account.AccountID == imperative.Account.AccountID);
                Accounts.Remove(account);

                account.Online = imperative.Online;
                Accounts.Add(account);
            });
        }

        public void NotifyPropertyChanged(string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            SelectedAccount = listView.SelectedItem as DisplayableAccount;
        }
    }
}
