using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using InvoicingApp.Models;

namespace InvoicingApp.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentStatus status)
            {
                switch (status)
                {
                    case PaymentStatus.Paid:
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    case PaymentStatus.PartiallyPaid:
                        return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    case PaymentStatus.Unpaid:
                        return new SolidColorBrush(Color.FromRgb(62, 108, 178)); // Blue
                    case PaymentStatus.Overdue:
                        return new SolidColorBrush(Color.FromRgb(226, 43, 43)); // Red
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Default gray
                }
            }
            else if (value is string statusString)
            {
                switch (statusString.ToLower())
                {
                    case "zapłacona":
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    case "częściowo zapłacona":
                        return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    case "oczekująca":
                        return new SolidColorBrush(Color.FromRgb(62, 108, 178)); // Blue
                    case "zaległa":
                        return new SolidColorBrush(Color.FromRgb(226, 43, 43)); // Red
                    case "robocza":
                        return new SolidColorBrush(Color.FromRgb(114, 114, 114)); // Gray
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Default gray
                }
            }

            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}