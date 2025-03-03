using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class InvoiceListViewModel : INotifyPropertyChanged
    {
        private readonly IInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;
        private readonly IPDFService _pdfService;

        private ObservableCollection<InvoiceListItem> _invoices;
        private string _searchText;
        private ICollectionView _filteredInvoices;
        private InvoiceListItem _selectedInvoice;
        private bool _isLoading;

        public InvoiceListViewModel(
        IInvoiceService invoiceService,
            INavigationService navigationService,
            IPDFService pdfService)
        {
            _invoiceService = invoiceService;
            _navigationService = navigationService;
            _pdfService = pdfService;

            // Initialize commands
            NewInvoiceCommand = new RelayCommand(CreateNewInvoice);
            EditInvoiceCommand = new RelayCommand(EditInvoice, CanEditInvoice);
            DeleteInvoiceCommand = new RelayCommand(DeleteInvoice, CanDeleteInvoice);
            MarkAsPaidCommand = new RelayCommand(MarkAsPaid, CanMarkAsPaid);
            ExportPdfCommand = new RelayCommand(ExportPdf, CanExportPdf);
            RefreshCommand = new RelayCommand(Refresh);

            // Load invoices
            LoadInvoicesAsync();
        }

        public ObservableCollection<InvoiceListItem> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                _filteredInvoices = CollectionViewSource.GetDefaultView(_invoices);
                _filteredInvoices.Filter = FilterInvoices;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredInvoices));
            }
        }

        public ICollectionView FilteredInvoices => _filteredInvoices;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _filteredInvoices?.Refresh();
                OnPropertyChanged();
            }
        }

        public InvoiceListItem SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
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

        // Statistics properties
        public int TotalInvoiceCount => Invoices?.Count ?? 0;
        public int PaidInvoiceCount => Invoices?.Count(i => i.IsPaid) ?? 0;
        public int UnpaidInvoiceCount => Invoices?.Count(i => !i.IsPaid) ?? 0;
        public decimal TotalAmount => Invoices?.Sum(i => i.TotalGross) ?? 0;
        public decimal PaidAmount => Invoices?.Where(i => i.IsPaid).Sum(i => i.TotalGross) ?? 0;
        public decimal UnpaidAmount => Invoices?.Where(i => !i.IsPaid).Sum(i => i.TotalGross) ?? 0;

        // Commands
        public ICommand NewInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand DeleteInvoiceCommand { get; }
        public ICommand MarkAsPaidCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void LoadInvoicesAsync()
        {
            IsLoading = true;

            try
            {
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
                        IsPaid = i.IsPaid,
                        PaymentDate = i.PaymentDate
                    }).OrderByDescending(i => i.InvoiceDate)
                );

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                // Handle error (log or show message)
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

        private void CreateNewInvoice()
        {
            _navigationService.NavigateTo<InvoiceEditorViewModel>();
        }

        private void EditInvoice()
        {
            if (SelectedInvoice != null)
            {
                _navigationService.NavigateTo<InvoiceEditorViewModel>(new NavigationParameter("InvoiceId", SelectedInvoice.Id));
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
                // Confirm deletion
                var result = System.Windows.MessageBox.Show(
                    $"Czy na pewno chcesz usunąć fakturę {SelectedInvoice.InvoiceNumber}?",
                    "Potwierdzenie usunięcia",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        await _invoiceService.DeleteInvoiceAsync(SelectedInvoice.Id);
                        Invoices.Remove(SelectedInvoice);
                        UpdateStatistics();
                    }
                    catch (Exception ex)
                    {
                        // Handle error
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
                    var invoice = await _invoiceService.GetInvoiceByIdAsync(SelectedInvoice.Id);
                    if (invoice != null)
                    {
                        invoice.IsPaid = true;
                        invoice.PaymentDate = DateTime.Now;

                        await _invoiceService.SaveInvoiceAsync(invoice);

                        // Update the list item
                        SelectedInvoice.IsPaid = true;
                        SelectedInvoice.PaymentDate = invoice.PaymentDate;

                        // Refresh the view
                        _filteredInvoices.Refresh();
                        UpdateStatistics();
                    }
                }
                catch (Exception ex)
                {
                    // Handle error
                }
            }
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
                    var invoice = await _invoiceService.GetInvoiceByIdAsync(SelectedInvoice.Id);
                    if (invoice != null)
                    {
                        var pdfPath = await _pdfService.GenerateInvoicePdfAsync(invoice);

                        // Open the PDF
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = pdfPath,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Handle error
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InvoiceListItem
    {
        public string Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalGross { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }

        public string Status => IsPaid ? "Zapłacona" : (DateTime.Now > DueDate ? "Zaległa" : "Oczekująca");
        public string FormattedDate => InvoiceDate.ToString("dd.MM.yyyy");
        public string FormattedDueDate => DueDate.ToString("dd.MM.yyyy");
        public string FormattedAmount => $"{TotalGross:N2} PLN";
    }
}