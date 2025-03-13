using System;
using System.Windows;
using InvoicingApp.Windows;

namespace InvoicingApp.Services
{
    public class DialogService : IDialogService
    {
        public DialogService()
        {

        }

        public void ShowError(string message, string title = "Błąd")
        {
            // Use notification for non-critical errors
            WindowNotificationService.ShowError(message, title);

            // For critical errors, use a modal dialog
            /*
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded)
                {
                    CustomDialogWindow.ShowError(mainWindow, message, title);
                }
            });
            */
        }

        public void ShowError(Exception ex, string title = "Błąd")
        {
            ShowError(ex.Message, title);
        }

        public void ShowInformation(string message, string title = "Informacja")
        {
            WindowNotificationService.ShowInformation(message, title);
        }

        public bool ShowQuestion(string message, string title = "Pytanie")
        {
            bool result = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded)
                {
                    result = CustomDialogWindow.ShowQuestion(mainWindow, message, title);
                }
                else
                {
                    result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                }
            });

            return result;
        }

        public void ShowWarning(string message, string title = "Ostrzeżenie")
        {
            WindowNotificationService.ShowWarning(message, title);
        }

        public void ShowSuccess(string message, string title = "Sukces")
        {
            WindowNotificationService.ShowSuccess(message, title);
        }
    }
}