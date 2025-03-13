using System;

namespace InvoicingApp.Services
{
    public interface IDialogService
    {
        void ShowError(string message, string title = "Błąd");
        void ShowError(Exception ex, string title = "Błąd");
        void ShowInformation(string message, string title = "Informacja");
        bool ShowQuestion(string message, string title = "Pytanie");
        void ShowWarning(string message, string title = "Ostrzeżenie");
        void ShowSuccess(string message, string title = "Sukces");
    }
}