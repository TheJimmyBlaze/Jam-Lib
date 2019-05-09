using ExampleClient.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ExampleClient.Converter
{
    class MessageBubbleSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DisplayableMessage.MessageType side = (DisplayableMessage.MessageType)value;
            if (side == DisplayableMessage.MessageType.Remote)
                return HorizontalAlignment.Left;
            if (side == DisplayableMessage.MessageType.Local)
                return HorizontalAlignment.Right;
            return HorizontalAlignment.Stretch;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
