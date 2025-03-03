using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using InvoicingApp.Services;
using InvoicingApp.ViewModels;

namespace InvoicingApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private string _activeView;

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // Initialize commands
            NavigateToInvoicesCommand = new RelayCommand(NavigateToInvoices);
            NavigateToClientsCommand = new RelayCommand(NavigateToClients);
            NavigateToReportsCommand = new RelayCommand(NavigateToReports);
            NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);

            // Default navigation to invoices
            NavigateToInvoices();
        }

        public string ActiveView
        {
            get => _activeView;
            set
            {
                _activeView = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToInvoicesCommand { get; }
        public ICommand NavigateToClientsCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        private void NavigateToInvoices()
        {
            _navigationService.NavigateTo<InvoiceListViewModel>();
            ActiveView = "Invoices";
        }

        private void NavigateToClients()
        {
            _navigationService.NavigateTo<ClientsViewModel>();
            ActiveView = "Clients";
        }

        private void NavigateToReports()
        {
            _navigationService.NavigateTo<ReportsViewModel>();
            ActiveView = "Reports";
        }

        private void NavigateToSettings()
        {
            _navigationService.NavigateTo<SettingsViewModel>();
            ActiveView = "Settings";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}