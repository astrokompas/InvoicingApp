using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
                InvoiceNumber = _invoiceService.GenerateNextInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                SellingDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Items = new List<InvoiceItem>(),
                Payments = new List<Payment>(),
                PaymentStatus = PaymentStatus.Unpaid
            };

            _availableClients = new ObservableCollection<Client>();
            _vatRates = new ObservableCollection<string>();

            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<InvoiceItem>(RemoveItem);
            SaveInvoiceCommand = new RelayCommand(SaveInvoice);
            SwitchModeCommand = new RelayCommand<string>(SwitchMode);
            CancelCommand = new RelayCommand(Cancel);
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

                if (CurrentInvoice.Items.Count == 0)
                {
                    AddItem();
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

            _currentInvoice.Items.Add(newItem);
            CalculateTotals();
        }

        private void RemoveItem(InvoiceItem item)
        {
            if (item != null)
            {
                _currentInvoice.Items.Remove(item);
                CalculateTotals();
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
                if (decimal.TryParse(item.VatRate.TrimEnd('%'), out decimal vatRate))
                {
                    vatRate = vatRate / 100;

                    decimal itemNet = item.Quantity * item.NetPrice;
                    decimal itemVat = itemNet * vatRate;

                    totalNet += itemNet;
                    totalVat += itemVat;

                    item.TotalNet = itemNet;
                    item.TotalVat = itemVat;
                    item.TotalGross = itemNet + itemVat;
                }
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