using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InvoicingApp.Converters
{
    public class BoolToActiveBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) // Green for active
                    : new SolidColorBrush(Color.FromRgb(226, 43, 43)); // Red for inactive
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}