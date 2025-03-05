using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class InvoiceListViewModel : BaseViewModel, IAsyncInitializable
    {
        private readonly IInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;
        private readonly IPDFService _pdfService;

        private ObservableCollection<InvoiceListItem> _invoices;
        private string _searchText;
        private ICollectionView _filteredInvoices;
        private InvoiceListItem _selectedInvoice;

        public InvoiceListViewModel(
            IInvoiceService invoiceService,
            INavigationService navigationService,
            IPDFService pdfService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _invoiceService = invoiceService;
            _navigationService = navigationService;
            _pdfService = pdfService;

            NewInvoiceCommand = new RelayCommand(CreateNewInvoice);
            EditInvoiceCommand = new RelayCommand(EditInvoice, CanEditInvoice);
            DeleteInvoiceCommand = new RelayCommand(DeleteInvoice, CanDeleteInvoice);
            MarkAsPaidCommand = new RelayCommand(MarkAsPaid, CanMarkAsPaid);
            ExportPdfCommand = new RelayCommand(ExportPdf, CanExportPdf);
            RefreshCommand = new RelayCommand(Refresh);
            AddPaymentCommand = new RelayCommand(AddPayment, CanAddPayment);
        }

        public async Task InitializeAsync()
        {
            await LoadInvoicesAsync();
        }

        public ObservableCollection<InvoiceListItem> Invoices
        {
            get => _invoices;
            set
            {
                if (SetProperty(ref _invoices, value))
                {
                    _filteredInvoices = CollectionViewSource.GetDefaultView(_invoices);
                    _filteredInvoices.Filter = FilterInvoices;
                    OnPropertyChanged(nameof(FilteredInvoices));
                    UpdateStatistics();
                }
            }
        }

        public ICollectionView FilteredInvoices => _filteredInvoices;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _filteredInvoices?.Refresh();
            }
        }

        public InvoiceListItem SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (SetProperty(ref _selectedInvoice, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public int TotalInvoiceCount => Invoices?.Count ?? 0;
        public int PaidInvoiceCount => Invoices?.Count(i => i.IsPaid) ?? 0;
        public int UnpaidInvoiceCount => Invoices?.Count(i => !i.IsPaid) ?? 0;
        public decimal TotalAmount => Invoices?.Sum(i => i.TotalGross) ?? 0;
        public decimal PaidAmount => Invoices?.Where(i => i.IsPaid).Sum(i => i.TotalGross) ?? 0;
        public decimal UnpaidAmount => Invoices?.Where(i => !i.IsPaid).Sum(i => i.TotalGross) ?? 0;

        public ICommand NewInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand DeleteInvoiceCommand { get; }
        public ICommand MarkAsPaidCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddPaymentCommand { get; }

        private async Task LoadInvoicesAsync()
        {
            try
            {
                IsLoading = true;
                var invoices = await _invoiceService.GetAllInvoicesAsync();

                Invoices = new ObservableCollection<InvoiceListItem>(
                    invoices.Select(i => new InvoiceListItem
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        ClientName = i.Client?.Name ?? "Nieznany klient",
                        InvoiceDate = i.InvoiceDate,
                        DueDate = i.DueDate,
                        TotalGross = i.TotalGross,
                        PaymentStatus = i.PaymentStatus,
                        PaymentDate = i.PaymentDate,
                        PaidAmount = i.PaidAmount,
                    }).OrderByDescending(i => i.InvoiceDate)
                );
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas ładowania faktur");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalInvoiceCount));
            OnPropertyChanged(nameof(PaidInvoiceCount));
            OnPropertyChanged(nameof(UnpaidInvoiceCount));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(PaidAmount));
            OnPropertyChanged(nameof(UnpaidAmount));
        }

        private bool FilterInvoices(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (item is InvoiceListItem invoice)
            {
                return invoice.InvoiceNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       invoice.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private async void CreateNewInvoice()
        {
            try
            {
                IsLoading = true;
                await _navigationService.NavigateToAsync<InvoiceEditorViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void EditInvoice()
        {
            if (SelectedInvoice != null)
            {
                try
                {
                    IsLoading = true;
                    await _navigationService.NavigateToAsync<InvoiceEditorViewModel>(
                        new NavigationParameter("InvoiceId", SelectedInvoice.Id));
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanEditInvoice()
        {
            return SelectedInvoice != null;
        }

        private async void DeleteInvoice()
        {
            if (SelectedInvoice != null)
            {
                if (DisplayQuestion(
                    $"Czy na pewno chcesz usunąć fakturę {SelectedInvoice.InvoiceNumber}?",
                    "Potwierdzenie usunięcia"))
                {
                    try
                    {
                        IsLoading = true;
                        await _invoiceService.DeleteInvoiceAsync(SelectedInvoice.Id);
                        Invoices.Remove(SelectedInvoice);
                        UpdateStatistics();
                        DisplayInformation("Faktura została usunięta pomyślnie.", "Usunięto");
                    }
                    catch (Exception ex)
                    {
                        DisplayError(ex, "Błąd podczas usuwania faktury");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private bool CanDeleteInvoice()
        {
            return SelectedInvoice != null;
        }

        private async void MarkAsPaid()
        {
            if (SelectedInvoice != null)
            {
                try
                {
                    IsLoading = true;
                    var invoice = await _invoiceService.GetInvoiceByIdAsync(SelectedInvoice.Id);
                    if (invoice != null)
                    {
                        var payment = new Payment
                        {
                            Date = DateTime.Now,
                            Amount = invoice.TotalGross,
                            Method = invoice.PaymentMethod,
                            Notes = "Płatność całkowita"
                        };

                        invoice.Payments.Add(payment);
                        invoice.PaymentStatus = PaymentStatus.Paid;

                        await _invoiceService.SaveInvoiceAsync(invoice);

                        SelectedInvoice.PaymentStatus = PaymentStatus.Paid;
                        SelectedInvoice.PaymentDate = DateTime.Now;
                        SelectedInvoice.PaidAmount = invoice.TotalGross;

                        _filteredInvoices.Refresh();
                        UpdateStatistics();

                        DisplayInformation("Faktura została oznaczona jako zapłacona.", "Zapłacono");
                    }
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas oznaczania faktury jako zapłaconej");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async void AddPayment()
        {
            if (SelectedInvoice != null)
            {
                try
                {
                    IsLoading = true;
                    await _navigationService.NavigateToAsync<AddPaymentViewModel>(
                        new NavigationParameter("InvoiceId", SelectedInvoice.Id));
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanAddPayment()
        {
            return SelectedInvoice != null && !SelectedInvoice.IsPaid;
        }

        private bool CanMarkAsPaid()
        {
            return SelectedInvoice != null && !SelectedInvoice.IsPaid;
        }

        private async void ExportPdf()
        {
            if (SelectedInvoice != null)
            {
                try
                {
                    IsLoading = true;
                    var invoice = await _invoiceService.GetInvoiceByIdAsync(SelectedInvoice.Id);
                    if (invoice != null)
                    {
                        var pdfPath = await _pdfService.GenerateInvoicePdfAsync(invoice);

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = pdfPath,
                            UseShellExecute = true
                        });

                        DisplayInformation("Faktura została wyeksportowana do PDF.", "Eksport PDF");
                    }
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas eksportowania faktury do PDF");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanExportPdf()
        {
            return SelectedInvoice != null;
        }

        private void Refresh()
        {
            LoadInvoicesAsync();
        }
    }
}