using System;
using System.Globalization;
using System.Windows.Data;

namespace InvoicingApp.Converters
{
    public class NullToAddEditClientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return "Dodaj klienta";
            }
            else
            {
                return "Edytuj klienta";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}