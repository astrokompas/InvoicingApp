using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly IReportService _reportService;
        private readonly IClientService _clientService;

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        private DateTime _endDate = DateTime.Today;
        private Client _selectedClient;
        private ReportSummary _reportSummary;
        private bool _isLoading;

        public ReportsViewModel(IReportService reportService, IClientService clientService)
        {
            _reportService = reportService;
            _clientService = clientService;

            RefreshReportCommand = new AsyncRelayCommand(RefreshReport);
            ExportReportCommand = new AsyncRelayCommand(ExportReport);

            LoadClientsAsync();
            RefreshReport();
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
            }
        }

        public ReportSummary ReportSummary
        {
            get => _reportSummary;
            set
            {
                _reportSummary = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();

        public string PaymentRatio => $"{ReportSummary?.PaidInvoices ?? 0}/{ReportSummary?.TotalInvoices ?? 0} faktur opłaconych";

        public ICommand RefreshReportCommand { get; }
        public ICommand ExportReportCommand { get; }

        private async Task LoadClientsAsync()
        {
            try
            {
                Clients.Clear();

                // Add "All Clients" option
                Clients.Add(new Client { Id = null, Name = "Wszyscy klienci" });

                var clients = await _clientService.GetAllClientsAsync();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                SelectedClient = Clients[0]; // Select "All Clients" by default
            }
            catch (Exception)
            {
                // Handle error
            }
        }

        private async Task RefreshReport()
        {
            try
            {
                IsLoading = true;

                ReportSummary = await _reportService.GenerateReportAsync(
                    StartDate,
                    EndDate,
                    SelectedClient?.Id
                );

                OnPropertyChanged(nameof(PaymentRatio));
            }
            catch (Exception)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportReport()
        {
            // Implementation of report export (PDF or Excel)
            // Not implemented in this sample
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for async commands
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object parameter)
        {
            if (_isExecuting)
                return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}