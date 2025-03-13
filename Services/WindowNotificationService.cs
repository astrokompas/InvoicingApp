using System;
using System.Windows;
using System.Windows.Media;
using InvoicingApp.Windows;

namespace InvoicingApp.Services
{
    public static class WindowNotificationService
    {
        private static int _activeNotifications = 0;
        private static readonly int MaxNotifications = 5;

        static WindowNotificationService()
        {

        }

        public static void ShowNotification(string message, string title, NotificationType type = NotificationType.Information)
        {

            if (_activeNotifications >= MaxNotifications)
            {
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var icon = GetIcon(type);
                    var color = GetColor(type);

                    Window mainWindow = Application.Current.MainWindow;
                    if (mainWindow == null)
                    {
                        return;
                    }

                    var window = new NotificationWindow(mainWindow, message, title, icon, color, 5000);

                    _activeNotifications++;
                    window.Closed += (s, e) => _activeNotifications--;

                    window.Show();
                });
            }
            catch (Exception ex)
            {

            }
        }

        public static void ShowInformation(string message, string title = "Informacja")
        {
            ShowNotification(message, title, NotificationType.Information);
        }

        public static void ShowSuccess(string message, string title = "Sukces")
        {
            ShowNotification(message, title, NotificationType.Success);
        }

        public static void ShowWarning(string message, string title = "Ostrzeżenie")
        {
            ShowNotification(message, title, NotificationType.Warning);
        }

        public static void ShowError(string message, string title = "Błąd")
        {
            ShowNotification(message, title, NotificationType.Error);
        }

        public static void ShowError(Exception ex, string title = "Błąd")
        {
            ShowError(ex.Message, title);
        }

        private static string GetIcon(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    return "✓";
                case NotificationType.Warning:
                    return "⚠";
                case NotificationType.Error:
                    return "❌";
                case NotificationType.Information:
                default:
                    return "ℹ";
            }
        }

        private static Color GetColor(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    return (Color)ColorConverter.ConvertFromString("#4CAF50");
                case NotificationType.Warning:
                    return (Color)ColorConverter.ConvertFromString("#FF9800");
                case NotificationType.Error:
                    return (Color)ColorConverter.ConvertFromString("#E22B2B");
                case NotificationType.Information:
                default:
                    return (Color)ColorConverter.ConvertFromString("#3E6CB2");
            }
        }
    }

    public enum NotificationType
    {
        Information,
        Success,
        Warning,
        Error
    }
}