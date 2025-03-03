using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using InvoicingApp.Models;

namespace InvoicingApp.ViewModels
{
    public class InvoiceEditorViewModel : INotifyPropertyChanged
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IClientService _clientService;
        private readonly ISettingsService _settingsService;

        private Invoice _currentInvoice;
        private ObservableCollection<Client> _availableClients;
        private ObservableCollection<string> _vatRates;
        private string _editMode = "InvoiceDetails"; // InvoiceDetails, Items, Payment

        public InvoiceEditorViewModel(
            IInvoiceService invoiceService,
            IClientService clientService,
            ISettingsService settingsService)
        {
            _invoiceService = invoiceService;
            _clientService = clientService;
            _settingsService = settingsService;

            // Initialize with a new invoice or load existing
            _currentInvoice = new Invoice
            {
                InvoiceNumber = _invoiceService.GenerateNextInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                SellingDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Items = new ObservableCollection<InvoiceItem>()
            };

            // Load clients and settings
            _availableClients = new ObservableCollection<Client>(_clientService.GetAllClients());
            _vatRates = new ObservableCollection<string>(_settingsService.GetVatRates());

            // Initialize commands
            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<InvoiceItem>(RemoveItem);
            SaveInvoiceCommand = new RelayCommand(SaveInvoice);
            SwitchModeCommand = new RelayCommand<string>(SwitchMode);

            // Add initial item if needed
            if (_currentInvoice.Items.Count == 0)
            {
                AddItem();
            }

            // Update totals
            CalculateTotals();
        }

        // Properties for binding
        public Invoice CurrentInvoice
        {
            get => _currentInvoice;
            set
            {
                _currentInvoice = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Client> AvailableClients
        {
            get => _availableClients;
            set
            {
                _availableClients = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> VatRates
        {
            get => _vatRates;
            set
            {
                _vatRates = value;
                OnPropertyChanged();
            }
        }

        public string EditMode
        {
            get => _editMode;
            set
            {
                _editMode = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand SwitchModeCommand { get; }

        // Methods
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

        private void SaveInvoice()
        {
            // Validate invoice
            if (string.IsNullOrWhiteSpace(_currentInvoice.InvoiceNumber))
            {
                // Show error
                return;
            }

            if (_currentInvoice.Client == null)
            {
                // Show error
                return;
            }

            if (_currentInvoice.Items.Count == 0)
            {
                // Show error
                return;
            }

            // Save invoice
            _invoiceService.SaveInvoice(_currentInvoice);

            // Navigate back or show success
        }

        private void SwitchMode(string mode)
        {
            if (mode != null)
            {
                EditMode = mode;
            }
        }

        public void CalculateTotals()
        {
            decimal totalNet = 0;
            decimal totalVat = 0;

            foreach (var item in _currentInvoice.Items)
            {
                // Parse VAT rate (e.g., "23%" to 0.23)
                if (decimal.TryParse(item.VatRate.TrimEnd('%'), out decimal vatRate))
                {
                    vatRate = vatRate / 100;

                    decimal itemNet = item.Quantity * item.NetPrice;
                    decimal itemVat = itemNet * vatRate;

                    totalNet += itemNet;
                    totalVat += itemVat;

                    // Update item totals
                    item.TotalNet = itemNet;
                    item.TotalVat = itemVat;
                    item.TotalGross = itemNet + itemVat;
                }
            }

            _currentInvoice.TotalNet = totalNet;
            _currentInvoice.TotalVat = totalVat;
            _currentInvoice.TotalGross = totalNet + totalVat;

            // Notify about changes
            OnPropertyChanged(nameof(CurrentInvoice));
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}