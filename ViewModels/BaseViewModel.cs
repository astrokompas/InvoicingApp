using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;
        protected readonly IDialogService _dialogService;

        public BaseViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                IsLoading = true;
                await action();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void HandleException(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");

            DisplayError(ex);
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