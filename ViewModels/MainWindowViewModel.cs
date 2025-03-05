using System;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Services;
using InvoicingApp.ViewModels;

namespace InvoicingApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private string _activeView;
        private bool _isInitialized = false;

        public MainWindowViewModel(
            INavigationService navigationService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _navigationService = navigationService;

            // Initialize commands
            NavigateToInvoicesCommand = new AsyncRelayCommand(NavigateToInvoicesAsync);
            NavigateToClientsCommand = new AsyncRelayCommand(NavigateToClientsAsync);
            NavigateToReportsCommand = new AsyncRelayCommand(NavigateToReportsAsync);
            NavigateToSettingsCommand = new AsyncRelayCommand(NavigateToSettingsAsync);
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

        public async Task NavigateToInvoicesAsync()
        {
            if (_isInitialized && ActiveView == "Invoices")
                return;

            try
            {
                IsLoading = true;
                await _navigationService.NavigateToAsync<InvoiceListViewModel>();
                ActiveView = "Invoices";
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                // Log or display error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToClientsAsync()
        {
            if (ActiveView == "Clients")
                return;

            try
            {
                IsLoading = true;
                await _navigationService.NavigateToAsync<ClientsViewModel>();
                ActiveView = "Clients";
            }
            catch (Exception ex)
            {
                // Log or display error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToReportsAsync()
        {
            if (ActiveView == "Reports")
                return;

            try
            {
                IsLoading = true;
                await _navigationService.NavigateToAsync<ReportsViewModel>();
                ActiveView = "Reports";
            }
            catch (Exception ex)
            {
                // Log or display error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToSettingsAsync()
        {
            if (ActiveView == "Settings")
                return;

            try
            {
                IsLoading = true;
                await _navigationService.NavigateToAsync<SettingsViewModel>();
                ActiveView = "Settings";
            }
            catch (Exception ex)
            {
                // Log or display error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}