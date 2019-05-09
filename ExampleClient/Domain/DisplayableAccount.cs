using JamLib.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ExampleClient.Domain
{
    public class DisplayableAccount
    {
        public Account Account { get; set; }
        public bool Online { get; set; }

        public bool Selected { get; set; }

        private ObservableCollection<DisplayableMessage> messages;
        public ObservableCollection<DisplayableMessage> Messages
        {
            get
            {
                if (messages == null)
                {
                    messages = new ObservableCollection<DisplayableMessage>();
                    messages.Add(new DisplayableMessage(DisplayableMessage.MessageType.System, this, DateTime.UtcNow,
                        string.Format("Begining of message history with {0} for this session.", Account.Username)));
                }
                return messages;
            }
            set { }
        }

        public string Initial
        {
            get { return Account?.Username.Substring(0, 1); }
            set { }
        }

        public string OnlineStatus
        {
            get
            {
                if (Online)
                    return "Online";
                return "Offline";
            }
            set { }
        }

        public Brush OnlineColour
        {
            get
            {
                if (Online)
                    return Brushes.Gray;
                return Brushes.LightGray;
            }
        }

        public Brush Colour
        {
            get
            {
                if (Account == null)
                    return null;

                Random random = new Random(Account.AccountID.GetHashCode());
                int r = (int)(random.NextDouble() * 100) + 100;
                int g = (int)(random.NextDouble() * 100) + 100;
                int b = (int)(random.NextDouble() * 100) + 100;

                return new SolidColorBrush(Color.FromArgb(255, (byte)r, (byte)g, (byte)b));
            }
            set { }
        }

        public DisplayableAccount(Account account, bool online)
        {
            Account = account;
            Online = online;
        }
    }
}
