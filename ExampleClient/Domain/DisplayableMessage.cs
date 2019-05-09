using System;

namespace ExampleClient.Domain
{
    public class DisplayableMessage
    {
        public enum MessageType { System, Remote, Local };
        public MessageType Type { get; set; }

        public DisplayableAccount Sender { get; set; }
        public DateTime SendTimeUTC { get; set; }
        public string Text { get; set; }

        public bool Seen { get; set; }

        public string SendTimeString
        {
            get
            {
                return SendTimeUTC.ToLocalTime().ToString("HH:mm dd/MM/yy");
            }
            set { }
        }

        public bool IsSystem { get { return Type == MessageType.System; }  set { } }
        public bool IsRemote { get { return Type == MessageType.Remote; } set { } }
        public bool IsLocal { get { return Type == MessageType.Local; } set { } }

        public DisplayableMessage(MessageType type, DisplayableAccount sender, DateTime sendTimeUTC, string text)
        {
            Type = type;
            Sender = sender;
            SendTimeUTC = sendTimeUTC;
            Text = text;

            if (Type == MessageType.Remote)
                Seen = false;
            else
                Seen = true;
        }
    }
}
