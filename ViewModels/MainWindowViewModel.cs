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
        private bool _isNavigating = false;

        public MainWindowViewModel(
            INavigationService navigationService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

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
            if (_isNavigating || (_isInitialized && ActiveView == "Invoices"))
                return;

            try
            {
                _isNavigating = true;
                IsLoading = true;
                await _navigationService.NavigateToAsync<InvoiceListViewModel>();
                ActiveView = "Invoices";
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                DisplayError("Navigation failed. Please try again.", "Navigation Error");
            }
            finally
            {
                IsLoading = false;
                _isNavigating = false;
            }
        }

        private async Task NavigateToClientsAsync()
        {
            if (_isNavigating || ActiveView == "Clients")
                return;

            try
            {
                _isNavigating = true;
                IsLoading = true;
                await _navigationService.NavigateToAsync<ClientsViewModel>();
                ActiveView = "Clients";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                DisplayError("Navigation failed. Please try again.", "Navigation Error");
            }
            finally
            {
                IsLoading = false;
                _isNavigating = false;
            }
        }

        private async Task NavigateToReportsAsync()
        {
            if (_isNavigating || ActiveView == "Reports")
                return;

            try
            {
                _isNavigating = true;
                IsLoading = true;
                await _navigationService.NavigateToAsync<ReportsViewModel>();
                ActiveView = "Reports";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                DisplayError("Navigation failed. Please try again.", "Navigation Error");
            }
            finally
            {
                IsLoading = false;
                _isNavigating = false;
            }
        }

        private async Task NavigateToSettingsAsync()
        {
            if (_isNavigating || ActiveView == "Settings")
                return;

            try
            {
                _isNavigating = true;
                IsLoading = true;
                await _navigationService.NavigateToAsync<SettingsViewModel>();
                ActiveView = "Settings";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                DisplayError("Navigation failed. Please try again.", "Navigation Error");
            }
            finally
            {
                IsLoading = false;
                _isNavigating = false;
            }
        }
    }
}