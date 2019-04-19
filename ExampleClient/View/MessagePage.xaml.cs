using ExampleClient.Domain;
using JamLib.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace ExampleClient.View
{
    /// <summary>
    /// Interaction logic for MessagePage.xaml
    /// </summary>
    public partial class MessagePage : Page, INotifyPropertyChanged
    {
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

        public event PropertyChangedEventHandler PropertyChanged;

        public MessagePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Accounts.Add(new DisplayableAccount(new Account()
            {
                AccountID = Guid.Parse("f29af9c5-c98f-4067-a3ea-f9baab8853f9"),
                LastUpdateUTC = DateTime.UtcNow,
                Username = "Baiias",
                Approved = true
            }));

            Accounts.Add(new DisplayableAccount(new Account()
            {
                AccountID = Guid.Parse("67a7f709-7f6d-45da-bde6-2284fdfd6750"),
                LastUpdateUTC = DateTime.UtcNow,
                Username = "Deadponys",
                Approved = false
            }));

            Accounts.Add(new DisplayableAccount(new Account()
            {
                AccountID = Guid.Parse("ad509e39-afa9-4b56-bfe6-6c9d70a50954"),
                LastUpdateUTC = DateTime.UtcNow,
                Username = "TheJimmyBlaze",
                Approved = true
            }));
        }

        public void NotifyPropertyChanged(string name = "")
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
