using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InvoicingApp.Converters
{
    public class BoolToActiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Aktywny" : "Nieaktywny";
            }
            return "Nieznany";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
