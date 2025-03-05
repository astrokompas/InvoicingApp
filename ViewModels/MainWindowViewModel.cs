using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private string _activeView;

        public MainWindowViewModel(
            INavigationService navigationService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _navigationService = navigationService;

            // Initialize commands using centralized command classes
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
            set => SetProperty(ref _activeView, value);
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
    }
}