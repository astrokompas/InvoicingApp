using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;
        protected readonly IDialogService _dialogService;

        public BaseViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected async Task RunCommandAsync(Func<Task> action)
        {
            try
            {
                IsLoading = true;
                await action();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void DisplayError(Exception ex, string title = "Błąd")
        {
            _dialogService.ShowError(ex, title);
        }

        protected void DisplayError(string message, string title = "Błąd")
        {
            _dialogService.ShowError(message, title);
        }

        protected void DisplayInformation(string message, string title = "Informacja")
        {
            _dialogService.ShowInformation(message, title);
        }

        protected bool DisplayQuestion(string message, string title = "Pytanie")
        {
            return _dialogService.ShowQuestion(message, title);
        }

        protected void DisplayWarning(string message, string title = "Ostrzeżenie")
        {
            _dialogService.ShowWarning(message, title);
        }
    }
}