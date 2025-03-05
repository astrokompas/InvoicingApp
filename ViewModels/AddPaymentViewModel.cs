using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class AddPaymentViewModel : BaseViewModel, IParameterizedViewModel, IAsyncInitializable
    {
        private readonly IInvoiceService _invoiceService;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;

        private Invoice _invoice;
        private decimal _paymentAmount;
        private DateTime _paymentDate = DateTime.Now;
        private string _paymentMethod = "Przelew";
        private string _paymentNotes;
        private ObservableCollection<string> _paymentMethods;
        private string _invoiceId;

        public AddPaymentViewModel(
            IInvoiceService invoiceService,
            INavigationService navigationService,
            ISettingsService settingsService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _invoiceService = invoiceService;
            _navigationService = navigationService;
            _settingsService = settingsService;

            // Initialize commands
            AddPaymentCommand = new AsyncRelayCommand(AddPayment, CanAddPayment);
            CancelCommand = new RelayCommand(Cancel);

            // Initialize empty collections
            _paymentMethods = new ObservableCollection<string>();
        }

        public Invoice Invoice
        {
            get => _invoice;
            set => SetProperty(ref _invoice, value);
        }

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                if (SetProperty(ref _paymentAmount, value))
                    ((AsyncRelayCommand)AddPaymentCommand).NotifyCanExecuteChanged();
            }
        }

        public DateTime PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public string PaymentNotes
        {
            get => _paymentNotes;
            set => SetProperty(ref _paymentNotes, value);
        }

        public ObservableCollection<string> PaymentMethods
        {
            get => _paymentMethods;
            set => SetProperty(ref _paymentMethods, value);
        }

        public ICommand AddPaymentCommand { get; }
        public ICommand CancelCommand { get; }

        public void ApplyParameter(NavigationParameter parameter)
        {
            if (parameter != null && parameter.Contains("InvoiceId"))
            {
                _invoiceId = parameter.Get<string>("InvoiceId");
            }
        }

        // IAsyncInitializable implementation
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // Load settings
                var settings = await _settingsService.GetSettingsAsync();
                PaymentMethods = new ObservableCollection<string>(settings.PaymentMethods);
                PaymentMethod = settings.DefaultPaymentMethod;

                // Load invoice if ID was provided
                if (!string.IsNullOrEmpty(_invoiceId))
                {
                    await LoadInvoiceAsync(_invoiceId);
                }
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

        private async Task LoadInvoiceAsync(string invoiceId)
        {
            // Load invoice
            Invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);

            // Set default payment amount to remaining amount
            PaymentAmount = Invoice?.RemainingAmount ?? 0;
        }

        private async Task AddPayment()
        {
            try
            {
                IsLoading = true;

                if (Invoice == null)
                {
                    DisplayError("Nie można dodać płatności - brak faktury.", "Błąd płatności");
                    return;
                }

                // Create new payment
                var payment = new Payment
                {
                    Date = PaymentDate,
                    Amount = PaymentAmount,
                    Method = PaymentMethod,
                    Notes = PaymentNotes
                };

                // Add payment to invoice
                Invoice.Payments.Add(payment);

                // Update payment status
                UpdatePaymentStatus();

                // Save invoice
                await _invoiceService.SaveInvoiceAsync(Invoice);

                // Show success message
                DisplayInformation("Płatność została dodana pomyślnie.", "Płatność dodana");

                // Go back
                _navigationService.GoBack();
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas dodawania płatności");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddPayment()
        {
            return Invoice != null &&
                   PaymentAmount > 0 &&
                   PaymentAmount <= Invoice.RemainingAmount;
        }

        private void UpdatePaymentStatus()
        {
            if (Invoice.PaidAmount + PaymentAmount >= Invoice.TotalGross)
            {
                Invoice.PaymentStatus = PaymentStatus.Paid;
            }
            else if (Invoice.PaidAmount + PaymentAmount > 0)
            {
                Invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
            }
            else
            {
                Invoice.PaymentStatus = DateTime.Now > Invoice.DueDate ?
                    PaymentStatus.Overdue : PaymentStatus.Unpaid;
            }
        }

        private void Cancel()
        {
            _navigationService.GoBack();
        }
    }
}