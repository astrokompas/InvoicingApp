﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace InvoicingApp.Converters
{
    public class NullToAddEditClientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if value is null or empty string
            if (value == null || string.IsNullOrEmpty(value as string))
            {
                return "Dodaj klienta";
            }

            return "Edytuj klienta";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}