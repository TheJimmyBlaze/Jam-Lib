using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleClient.Domain
{
    public class MessageSession
    {
        private ObservableCollection<DisplayableMessage> messages;
        public ObservableCollection<DisplayableMessage> Messages
        {
            get
            {
                if (messages == null)
                    messages = new ObservableCollection<DisplayableMessage>();
                return messages;
            }
            set { }
        }
        
        public MessageSession(DisplayableMessage firstMessage)
        {
            Messages.Add(firstMessage);
        }
    }
}
