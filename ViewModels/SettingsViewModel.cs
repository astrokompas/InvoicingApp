using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvoicingApp.Models;
using InvoicingApp.Services;
using Microsoft.Win32;

namespace InvoicingApp.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;

        private AppSettings _settings;
        private ObservableCollection<string> _vatRates;
        private ObservableCollection<string> _currencies;
        private bool _isLoading;
        private string _newVatRate;
        private string _newCurrency;
        private string _logoPath;
        private bool _hasUnsavedChanges;

        public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService)
        {
            _settingsService = settingsService;
            _navigationService = navigationService;

            // Initialize commands
            SaveCompanyDataCommand = new RelayCommand(SaveCompanyData, () => HasUnsavedChanges);
            SaveSettingsCommand = new RelayCommand(SaveSettings, () => HasUnsavedChanges);
            SelectLogoCommand = new RelayCommand(SelectLogo);
            AddVatRateCommand = new RelayCommand(AddVatRate, CanAddVatRate);
            AddCurrencyCommand = new RelayCommand(AddCurrency, CanAddCurrency);
            RemoveVatRateCommand = new RelayCommand<string>(RemoveVatRate);
            RemoveCurrencyCommand = new RelayCommand<string>(RemoveCurrency);
            BackCommand = new RelayCommand(NavigateBack, () => !HasUnsavedChanges || ConfirmNavigateAway());

            // Load settings
            LoadSettingsAsync();
        }

        public AppSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
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

        public ObservableCollection<string> Currencies
        {
            get => _currencies;
            set
            {
                _currencies = value;
                OnPropertyChanged();
            }
        }

        public string NewVatRate
        {
            get => _newVatRate;
            set
            {
                _newVatRate = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string NewCurrency
        {
            get => _newCurrency;
            set
            {
                _newCurrency = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string LogoPath
        {
            get => _logoPath;
            set
            {
                _logoPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLogo));
                HasUnsavedChanges = true;
            }
        }

        public bool HasLogo => !string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath);

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Company properties for easy binding
        public string CompanyName
        {
            get => Settings?.CompanyName;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyName = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public string CompanyAddress
        {
            get => Settings?.CompanyAddress;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyAddress = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public string CompanyTaxID
        {
            get => Settings?.CompanyTaxID;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyTaxID = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public string CompanyEmail
        {
            get => Settings?.CompanyEmail;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyEmail = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public string CompanyPhone
        {
            get => Settings?.CompanyPhone;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyPhone = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public string CompanyBankAccount
        {
            get => Settings?.CompanyBankAccount;
            set
            {
                if (Settings != null)
                {
                    Settings.CompanyBankAccount = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        public int InvoiceRetentionDays
        {
            get => Settings?.InvoiceRetentionDays ?? 0;
            set
            {
                if (Settings != null)
                {
                    Settings.InvoiceRetentionDays = value;
                    OnPropertyChanged();
                    HasUnsavedChanges = true;
                }
            }
        }

        // Retention period options
        public ObservableCollection<RetentionOption> RetentionOptions { get; } = new ObservableCollection<RetentionOption>
        {
            new RetentionOption { Days = 0, DisplayName = "Nigdy nie usuwaj" },
            new RetentionOption { Days = 90, DisplayName = "90 dni" },
            new RetentionOption { Days = 180, DisplayName = "180 dni" },
            new RetentionOption { Days = 270, DisplayName = "270 dni" },
            new RetentionOption { Days = 360, DisplayName = "360 dni" }
        };

        // Commands
        public ICommand SaveCompanyDataCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand SelectLogoCommand { get; }
        public ICommand AddVatRateCommand { get; }
        public ICommand AddCurrencyCommand { get; }
        public ICommand RemoveVatRateCommand { get; }
        public ICommand RemoveCurrencyCommand { get; }
        public ICommand BackCommand { get; }

        private async void LoadSettingsAsync()
        {
            IsLoading = true;

            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                Settings = settings;

                LogoPath = Settings.CompanyLogoPath;

                // Initialize collections
                VatRates = new ObservableCollection<string>(settings.VatRates);
                Currencies = new ObservableCollection<string>(settings.Currencies);

                // Reset unsaved changes flag
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                // Handle error
                System.Windows.MessageBox.Show($"Błąd podczas ładowania ustawień: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SaveCompanyData()
        {
            if (Settings != null)
            {
                try
                {
                    Settings.CompanyLogoPath = LogoPath;

                    await _settingsService.SaveSettingsAsync(Settings);
                    HasUnsavedChanges = false;

                    System.Windows.MessageBox.Show("Dane firmy zostały zapisane.", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Handle error
                    System.Windows.MessageBox.Show($"Błąd podczas zapisywania danych firmy: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private async void SaveSettings()
        {
            if (Settings != null)
            {
                try
                {
                    // Update settings with current collections
                    Settings.VatRates = new List<string>(VatRates);
                    Settings.Currencies = new List<string>(Currencies);

                    await _settingsService.SaveSettingsAsync(Settings);
                    HasUnsavedChanges = false;

                    System.Windows.MessageBox.Show("Ustawienia zostały zapisane.", "Sukces", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Handle error
                    System.Windows.MessageBox.Show($"Błąd podczas zapisywania ustawień: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void SelectLogo()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Pliki obrazów|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Wybierz logo firmy"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    LogoPath = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                // Handle error
                System.Windows.MessageBox.Show($"Błąd podczas wybierania logo: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AddVatRate()
        {
            if (!string.IsNullOrWhiteSpace(NewVatRate))
            {
                if (!VatRates.Contains(NewVatRate))
                {
                    VatRates.Add(NewVatRate);
                    HasUnsavedChanges = true;
                }

                NewVatRate = string.Empty;
            }
        }

        private bool CanAddVatRate()
        {
            return !string.IsNullOrWhiteSpace(NewVatRate);
        }

        private void AddCurrency()
        {
            if (!string.IsNullOrWhiteSpace(NewCurrency))
            {
                if (!Currencies.Contains(NewCurrency))
                {
                    Currencies.Add(NewCurrency);
                    HasUnsavedChanges = true;
                }

                NewCurrency = string.Empty;
            }
        }

        private bool CanAddCurrency()
        {
            return !string.IsNullOrWhiteSpace(NewCurrency);
        }

        private void RemoveVatRate(string vatRate)
        {
            if (!string.IsNullOrEmpty(vatRate) && VatRates.Contains(vatRate))
            {
                VatRates.Remove(vatRate);
                HasUnsavedChanges = true;
            }
        }

        private void RemoveCurrency(string currency)
        {
            if (!string.IsNullOrEmpty(currency) && Currencies.Contains(currency))
            {
                Currencies.Remove(currency);
                HasUnsavedChanges = true;
            }
        }

        private void NavigateBack()
        {
            _navigationService.GoBack();
        }

        private bool ConfirmNavigateAway()
        {
            if (HasUnsavedChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "Masz niezapisane zmiany. Czy na pewno chcesz wyjść bez zapisywania?",
                    "Niezapisane zmiany",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                return result == System.Windows.MessageBoxResult.Yes;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RetentionOption
    {
        public int Days { get; set; }
        public string DisplayName { get; set; }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}