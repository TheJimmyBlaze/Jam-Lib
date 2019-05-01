using ExampleClient.Domain;
using ExampleServer.Network.Data;
using JamLib.Database;
using JamLib.Packet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private MessageSession selectedMessageSession;
        public MessageSession SelectedMessageSession
        {
            get { return selectedMessageSession; }
            set
            {
                selectedMessageSession = value;
                NotifyPropertyChanged(nameof(SelectedMessageSession));
            }
        }

        private ObservableCollection<MessageSession> messageSessions;
        public ObservableCollection<MessageSession> MessageSessions
        {
            get
            {
                if (messageSessions == null)
                    messageSessions = new ObservableCollection<MessageSession>();
                return messageSessions;
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

            //TODO: remove this test code.
            Account test = new Account()
            {
                Username = "Ding",
                AccountID = Guid.NewGuid()
            };

            MessageSession testSession = new MessageSession(new DisplayableMessage(DisplayableMessage.MessageType.System, new DisplayableAccount(test, true), DateTime.UtcNow,
                string.Format("Begining of message history with {0} for this session.", test.Username)));
            testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Remote, new DisplayableAccount(test, true), DateTime.UtcNow,
                "This is an example test message from a remote account. This message is very long, and artificially lengthened. I want this message to span multiple lines to test out the text wrapping functionality."));
            testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Remote, new DisplayableAccount(test, true), DateTime.UtcNow,
                "Here is another example message. This one is shorter."));
            testSession.Messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.Local, LoggedInAccount, DateTime.UtcNow,
                "This is a similar example message, this time from the local account."));

            MessageSessions.Add(testSession);
            SelectedMessageSession = MessageSessions[0];
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
    }
}
