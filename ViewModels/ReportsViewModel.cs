using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class ReportsViewModel : BaseViewModel, IAsyncInitializable
    {
        private readonly IReportService _reportService;
        private readonly IClientService _clientService;
        private readonly ReportPDFService _reportPDFService;

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        private DateTime _endDate = DateTime.Today;
        private Client _selectedClient;
        private ReportSummary _reportSummary;

        public ReportsViewModel(
            IReportService reportService,
            IClientService clientService,
            ISettingsService settingsService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _reportService = reportService;
            _clientService = clientService;
            _reportPDFService = new ReportPDFService(settingsService);

            RefreshReportCommand = new AsyncRelayCommand(ExecuteRefreshReport);
            ExportReportCommand = new AsyncRelayCommand(ExportReport);

            Clients = new ObservableCollection<Client>();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadClientsAsync();
                await RefreshReport(false);
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas inicjalizacji");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set => SetProperty(ref _selectedClient, value);
        }

        public ReportSummary ReportSummary
        {
            get => _reportSummary;
            set
            {
                if (SetProperty(ref _reportSummary, value))
                    OnPropertyChanged(nameof(PaymentRatio));
            }
        }

        public ObservableCollection<Client> Clients { get; }

        public string PaymentRatio => $"{ReportSummary?.PaidInvoices ?? 0}/{ReportSummary?.TotalInvoices ?? 0} faktur opłaconych";

        public ICommand RefreshReportCommand { get; }
        public ICommand ExportReportCommand { get; }

        private async Task LoadClientsAsync()
        {
            try
            {
                Clients.Clear();

                Clients.Add(new Client { Id = null, Name = "Wszyscy klienci" });

                var clients = await _clientService.GetAllClientsAsync();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                SelectedClient = Clients[0];
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas ładowania listy klientów");
            }
        }

        private async Task ExecuteRefreshReport()
        {
            await RefreshReport(true);
        }

        private async Task RefreshReport(bool showSuccessMessage = true)
        {
            try
            {
                IsLoading = true;

                ReportSummary = await _reportService.GenerateReportAsync(
                    StartDate,
                    EndDate,
                    SelectedClient?.Id
                );

                if (showSuccessMessage)
                {
                    DisplayInformation("Raport został wygenerowany pomyślnie.", "Raport");
                }
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas generowania raportu");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportReport()
        {
            try
            {
                IsLoading = true;

                if (ReportSummary == null)
                {
                    DisplayWarning(
                        "Brak danych do eksportu. Proszę wygenerować raport.",
                        "Eksport raportu");
                    return;
                }

                var pdfPath = await _reportPDFService.GenerateReportPdfAsync(ReportSummary);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });

                DisplayInformation("Raport został wyeksportowany do PDF.", "Eksport PDF");
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas eksportu raportu");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}