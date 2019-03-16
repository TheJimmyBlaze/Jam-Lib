using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Cryptography;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
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

namespace SandBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            const int HASH_ITTERATIONS = 500000;
            byte[] pepper = Encoding.ASCII.GetBytes("This is a test pepper, should probably make a better one than this lol!");
            SHA256HashFactory hashFactory = new SHA256HashFactory(HASH_ITTERATIONS, pepper);

            Account account = AccountManagement.CreateAccount("Jam", "newPassword1", hashFactory, true);

            JamServerEntities context = new JamServerEntities();
            context.Accounts.Add(account);
            context.SaveChanges();

            stopwatch.Stop();
            long milliseconds = stopwatch.ElapsedMilliseconds;

            bool valid = hashFactory.ValidateString("newPassword1", account.AccountAccessCodes.First().AccessCode);
            bool invalid = hashFactory.ValidateString("dingdong", account.AccountAccessCodes.First().AccessCode);
        }
    }
}
