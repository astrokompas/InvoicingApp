using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingApp.Services
{
    public interface IDialogService
    {
        void ShowError(string message, string title = "Błąd");
        void ShowError(Exception ex, string title = "Błąd");
        void ShowInformation(string message, string title = "Informacja");
        bool ShowQuestion(string message, string title = "Pytanie");
        void ShowWarning(string message, string title = "Ostrzeżenie");
    }
}
