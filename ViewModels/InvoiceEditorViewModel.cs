using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Converters;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class InvoiceEditorViewModel : BaseViewModel, IAsyncInitializable, IParameterizedViewModel
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IClientService _clientService;
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;

        private Invoice _currentInvoice;
        private ObservableCollection<Client> _availableClients;
        private ObservableCollection<string> _vatRates;
        private string _editMode = "InvoiceDetails";
        private ObservableCollection<string> _paymentDeadlineOptions;
        private string _selectedPaymentDeadline;
        private bool _isCustomDeadline;
        private ObservableCollection<InvoiceItem> _invoiceItems;
        private ObservableCollection<string> _paymentMethods;

        private string _companyName;
        private string _companyAddress;
        private string _companyPhone;
        private string _companyEmail;
        private string _companyNIP;

        public InvoiceEditorViewModel(
            IInvoiceService invoiceService,
            IClientService clientService,
            ISettingsService settingsService,
            INavigationService navigationService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _invoiceService = invoiceService;
            _clientService = clientService;
            _settingsService = settingsService;
            _navigationService = navigationService;

            _currentInvoice = new Invoice
            {
                InvoiceNumber = "Generowanie...",
                InvoiceDate = DateTime.Now,
                SellingDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Items = new List<InvoiceItem>(),
                Payments = new List<Payment>(),
                PaymentStatus = PaymentStatus.Unpaid
            };

            _availableClients = new ObservableCollection<Client>();
            _vatRates = new ObservableCollection<string>();
            _invoiceItems = new ObservableCollection<InvoiceItem>();
            _paymentMethods = new ObservableCollection<string>
            {
                "Przelew",
                "Gotówka"
            };

            InitializePaymentDeadlineOptions();

            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<InvoiceItem>(RemoveItem);
            SaveInvoiceCommand = new RelayCommand(SaveInvoice);
            SwitchModeCommand = new RelayCommand<string>(SwitchMode);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ObservableCollection<string> PaymentMethods
        {
            get => _paymentMethods;
            set => SetProperty(ref _paymentMethods, value);
        }

        public ObservableCollection<InvoiceItem> InvoiceItems
        {
            get => _invoiceItems;
            set => SetProperty(ref _invoiceItems, value);
        }

        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        public string CompanyAddress
        {
            get => _companyAddress;
            set => SetProperty(ref _companyAddress, value);
        }

        public string CompanyPhone
        {
            get => _companyPhone;
            set => SetProperty(ref _companyPhone, value);
        }

        public string CompanyEmail
        {
            get => _companyEmail;
            set => SetProperty(ref _companyEmail, value);
        }

        public string CompanyNIP
        {
            get => _companyNIP;
            set => SetProperty(ref _companyNIP, value);
        }

        private async Task LoadCompanySettingsAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            CompanyName = settings.CompanyName;
            CompanyAddress = settings.CompanyAddress;
            CompanyPhone = settings.CompanyPhone;
            CompanyEmail = settings.CompanyEmail;
            CompanyNIP = settings.CompanyTaxID;

            // Auto-populate bank account if it's not already set
            if (string.IsNullOrEmpty(CurrentInvoice.BankAccount))
            {
                CurrentInvoice.BankAccount = settings.CompanyBankAccount;
            }
        }

        public ObservableCollection<string> PaymentDeadlineOptions
        {
            get => _paymentDeadlineOptions;
            set => SetProperty(ref _paymentDeadlineOptions, value);
        }

        public string SelectedPaymentDeadline
        {
            get => _selectedPaymentDeadline;
            set
            {
                if (SetProperty(ref _selectedPaymentDeadline, value))
                {
                    UpdateDueDateFromDeadline();
                }
            }
        }

        public bool IsCustomDeadline
        {
            get => _isCustomDeadline;
            set => SetProperty(ref _isCustomDeadline, value);
        }

        private void InitializePaymentDeadlineOptions()
        {
            PaymentDeadlineOptions = new ObservableCollection<string>
    {
        "3 dni",
        "7 dni",
        "14 dni",
        "21 dni",
        "30 dni",
        "Data niestandardowa"
    };

            // Set default to 14 days
            SelectedPaymentDeadline = PaymentDeadlineOptions[2];
        }

        private void UpdateDueDateFromDeadline()
        {
            if (SelectedPaymentDeadline == null || CurrentInvoice == null)
                return;

            if (SelectedPaymentDeadline == "Data niestandardowa")
            {
                IsCustomDeadline = true;
                // Keep current date (let user pick)
            }
            else
            {
                IsCustomDeadline = false;

                // Parse days from selection - account for the format with parentheses
                string daysText = SelectedPaymentDeadline.Split(' ')[0];
                if (int.TryParse(daysText, out int days))
                {
                    CurrentInvoice.DueDate = CurrentInvoice.InvoiceDate.AddDays(days);
                    OnPropertyChanged(nameof(CurrentInvoice));
                }
            }
        }

        public void OnPaymentDeadlineSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDueDateFromDeadline();
        }



        public void ApplyParameter(NavigationParameter parameter)
        {
            if (parameter != null && parameter.Contains("InvoiceId"))
            {
                string invoiceId = parameter.Get<string>("InvoiceId");
                LoadInvoiceAsync(invoiceId);
            }
        }

        public async Task InitializeAsync()
        {
            await RunCommandAsync(async () =>
            {
                var settings = await _settingsService.GetSettingsAsync();
                VatRates = new ObservableCollection<string>(settings.VatRates);

                var clients = await _clientService.GetAllClientsAsync();
                AvailableClients = new ObservableCollection<Client>(clients);

                // Load company settings
                await LoadCompanySettingsAsync();

                // Only generate invoice number for new invoices (not loaded ones)
                if (CurrentInvoice != null && CurrentInvoice.InvoiceNumber == "Generowanie...")
                {
                    CurrentInvoice.InvoiceNumber = await _invoiceService.GenerateNextInvoiceNumberAsync();
                }

                InvoiceItems.Clear();
                foreach (var item in CurrentInvoice.Items)
                {
                    InvoiceItems.Add(item);
                }

                UpdateItemIndexes();

                if (CurrentInvoice.Items.Count == 0)
                {
                    AddItem();
                }

                if (string.IsNullOrEmpty(CurrentInvoice.PaymentMethod))
                {
                    CurrentInvoice.PaymentMethod = "Przelew";
                }

                CalculateTotals();
            });
        }

        private async void LoadInvoiceAsync(string invoiceId)
        {
            await RunCommandAsync(async () =>
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    CurrentInvoice = invoice;
                    CalculateTotals();
                }
                else
                {
                    DisplayError("Nie znaleziono faktury o podanym identyfikatorze", "Błąd ładowania");
                }
            });
        }

        public string AmountInWords
        {
            get
            {
                if (CurrentInvoice == null)
                    return string.Empty;

                return NumberToWordsConverter.ConvertToWords(CurrentInvoice.TotalGross);
            }
        }

        public Invoice CurrentInvoice
        {
            get => _currentInvoice;
            set => SetProperty(ref _currentInvoice, value);
        }

        public ObservableCollection<Client> AvailableClients
        {
            get => _availableClients;
            set => SetProperty(ref _availableClients, value);
        }

        public ObservableCollection<string> VatRates
        {
            get => _vatRates;
            set => SetProperty(ref _vatRates, value);
        }

        public string EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand CancelCommand { get; }

        private void AddItem()
        {
            var newItem = new InvoiceItem
            {
                Description = "Nowa pozycja",
                Quantity = 1,
                NetPrice = 0,
                VatRate = _vatRates.FirstOrDefault() ?? "23%"
            };

            // Add to both collections
            _currentInvoice.Items.Add(newItem);
            InvoiceItems.Add(newItem);

            // Update indexes
            UpdateItemIndexes();

            // Calculate totals after adding
            CalculateTotals();
        }

        private void RemoveItem(InvoiceItem item)
        {
            if (item != null)
            {
                // Remove from both collections
                _currentInvoice.Items.Remove(item);
                InvoiceItems.Remove(item);

                // Update indexes
                UpdateItemIndexes();

                // Recalculate totals
                CalculateTotals();
            }
        }

        private void UpdateItemIndexes()
        {
            int index = 1;
            foreach (var item in InvoiceItems)
            {
                // Set a custom property for the index display
                item.Index = index++;
            }
        }

        private async void SaveInvoice()
        {
            if (string.IsNullOrWhiteSpace(_currentInvoice.InvoiceNumber))
            {
                DisplayWarning("Numer faktury jest wymagany", "Błąd walidacji");
                return;
            }

            if (_currentInvoice.Client == null)
            {
                DisplayWarning("Klient jest wymagany", "Błąd walidacji");
                return;
            }

            if (_currentInvoice.Items.Count == 0)
            {
                DisplayWarning("Faktura musi zawierać przynajmniej jedną pozycję", "Błąd walidacji");
                return;
            }

            try
            {
                IsLoading = true;

                _currentInvoice.ClientId = _currentInvoice.Client.Id;

                await _invoiceService.SaveInvoiceAsync(_currentInvoice);

                DisplayInformation("Faktura została zapisana pomyślnie", "Zapisano");

                _navigationService.GoBack();
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas zapisywania faktury");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SwitchMode(string mode)
        {
            if (mode != null)
            {
                EditMode = mode;
            }
        }

        private void Cancel()
        {
            _navigationService.GoBack();
        }

        public void CalculateTotals()
        {
            decimal totalNet = 0;
            decimal totalVat = 0;

            foreach (var item in _currentInvoice.Items)
            {
                decimal vatRate = 0;
                if (decimal.TryParse(item.VatRate.TrimEnd('%'), out decimal parsedRate))
                {
                    vatRate = parsedRate / 100;
                }

                decimal itemNet = item.Quantity * item.NetPrice;
                decimal itemVat = itemNet * vatRate;

                totalNet += itemNet;
                totalVat += itemVat;

                item.TotalNet = itemNet;
                item.TotalVat = itemVat;
                item.TotalGross = itemNet + itemVat;

                // Property change notification for item totals
                OnPropertyChanged(nameof(InvoiceItems));
            }

            _currentInvoice.TotalNet = totalNet;
            _currentInvoice.TotalVat = totalVat;
            _currentInvoice.TotalGross = totalNet + totalVat;

            UpdatePaymentStatus();

            OnPropertyChanged(nameof(CurrentInvoice));
            OnPropertyChanged(nameof(AmountInWords));
        }

        private void UpdatePaymentStatus()
        {
            if (_currentInvoice == null)
                return;

            decimal paidAmount = 0;
            foreach (var payment in _currentInvoice.Payments)
            {
                paidAmount += payment.Amount;
            }

            if (paidAmount >= _currentInvoice.TotalGross)
            {
                _currentInvoice.PaymentStatus = PaymentStatus.Paid;
            }
            else if (paidAmount > 0)
            {
                _currentInvoice.PaymentStatus = PaymentStatus.PartiallyPaid;
            }
            else if (DateTime.Now > _currentInvoice.DueDate)
            {
                _currentInvoice.PaymentStatus = PaymentStatus.Overdue;
            }
            else
            {
                _currentInvoice.PaymentStatus = PaymentStatus.Unpaid;
            }
        }
    }
}