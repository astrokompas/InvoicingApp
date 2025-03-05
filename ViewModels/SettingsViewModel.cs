using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InvoicingApp.Commands;
using InvoicingApp.Models;
using InvoicingApp.Services;
using Microsoft.Win32;
using static InvoicingApp.Models.AppSettings;

namespace InvoicingApp.ViewModels
{
    public class SettingsViewModel : BaseViewModel, IAsyncInitializable
    {
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;
        private readonly IBackupService _backupService;

        private AppSettings _settings;
        private ObservableCollection<string> _vatRates;
        private ObservableCollection<string> _currencies;
        private string _newVatRate;
        private string _newCurrency;
        private string _logoPath;
        private bool _hasUnsavedChanges;

        public SettingsViewModel(
            ISettingsService settingsService,
            INavigationService navigationService,
            IDialogService dialogService,
            IBackupService backupService)
            : base(dialogService)
        {
            _settingsService = settingsService;
            _navigationService = navigationService;
            _backupService = backupService;

            // Use centralized command classes - no more duplicate implementations
            SaveCompanyDataCommand = new RelayCommand(SaveCompanyData, () => HasUnsavedChanges);
            SaveSettingsCommand = new RelayCommand(SaveSettings, () => HasUnsavedChanges);
            SelectLogoCommand = new RelayCommand(SelectLogo);
            AddVatRateCommand = new RelayCommand(AddVatRate, CanAddVatRate);
            AddCurrencyCommand = new RelayCommand(AddCurrency, CanAddCurrency);
            RemoveVatRateCommand = new RelayCommand<string>(RemoveVatRate);
            RemoveCurrencyCommand = new RelayCommand<string>(RemoveCurrency);
            BackCommand = new RelayCommand(NavigateBack, () => !HasUnsavedChanges || ConfirmNavigateAway());
            BackupDataCommand = new AsyncRelayCommand(BackupData);
            RestoreDataCommand = new AsyncRelayCommand(RestoreData);

            // Initialize empty collections
            _vatRates = new ObservableCollection<string>();
            _currencies = new ObservableCollection<string>();
        }

        // IAsyncInitializable implementation
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadSettingsAsync();
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Failed to initialize settings");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public AppSettings Settings
        {
            get => _settings;
            set
            {
                if (SetProperty(ref _settings, value))
                    HasUnsavedChanges = true;
            }
        }

        public ObservableCollection<string> VatRates
        {
            get => _vatRates;
            set => SetProperty(ref _vatRates, value);
        }

        public ObservableCollection<string> Currencies
        {
            get => _currencies;
            set => SetProperty(ref _currencies, value);
        }

        public string NewVatRate
        {
            get => _newVatRate;
            set
            {
                if (SetProperty(ref _newVatRate, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string NewCurrency
        {
            get => _newCurrency;
            set
            {
                if (SetProperty(ref _newCurrency, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string LogoPath
        {
            get => _logoPath;
            set
            {
                if (SetProperty(ref _logoPath, value))
                {
                    OnPropertyChanged(nameof(HasLogo));
                    HasUnsavedChanges = true;
                }
            }
        }

        public bool HasLogo => !string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath);

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (SetProperty(ref _hasUnsavedChanges, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        // Company properties for easy binding
        public string CompanyName
        {
            get => Settings?.CompanyName;
            set
            {
                if (Settings != null && Settings.CompanyName != value)
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
                if (Settings != null && Settings.CompanyAddress != value)
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
                if (Settings != null && Settings.CompanyTaxID != value)
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
                if (Settings != null && Settings.CompanyEmail != value)
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
                if (Settings != null && Settings.CompanyPhone != value)
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
                if (Settings != null && Settings.CompanyBankAccount != value)
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
                if (Settings != null && Settings.InvoiceRetentionDays != value)
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
        public ICommand BackupDataCommand { get; }
        public ICommand RestoreDataCommand { get; }

        private async Task LoadSettingsAsync()
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

        private async void SaveCompanyData()
        {
            if (Settings != null)
            {
                try
                {
                    IsLoading = true;
                    Settings.CompanyLogoPath = LogoPath;

                    await _settingsService.SaveSettingsAsync(Settings);
                    HasUnsavedChanges = false;

                    DisplayInformation("Dane firmy zostały zapisane.", "Zapisano");
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas zapisywania danych firmy");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async void SaveSettings()
        {
            if (Settings != null)
            {
                try
                {
                    IsLoading = true;
                    // Update settings with current collections
                    Settings.VatRates = new List<string>(VatRates);
                    Settings.Currencies = new List<string>(Currencies);

                    await _settingsService.SaveSettingsAsync(Settings);
                    HasUnsavedChanges = false;

                    DisplayInformation("Ustawienia zostały zapisane.", "Zapisano");
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas zapisywania ustawień");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task SaveSettingsAsync()
        {
            if (Settings != null)
            {
                try
                {
                    IsLoading = true;
                    // Update settings with current collections
                    Settings.VatRates = new List<string>(VatRates);
                    Settings.Currencies = new List<string>(Currencies);
                    Settings.CompanyLogoPath = LogoPath;

                    await _settingsService.SaveSettingsAsync(Settings);
                    HasUnsavedChanges = false;
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas zapisywania ustawień");
                }
                finally
                {
                    IsLoading = false;
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
                DisplayError(ex, "Błąd podczas wybierania logo");
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

        private async Task BackupData()
        {
            try
            {
                IsLoading = true;

                if (HasUnsavedChanges)
                {
                    if (DisplayQuestion("Masz niezapisane zmiany. Czy chcesz je zapisać przed utworzeniem kopii zapasowej?", "Niezapisane zmiany"))
                    {
                        await SaveSettingsAsync();
                    }
                }

                await _backupService.BackupDataAsync();
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas tworzenia kopii zapasowej");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RestoreData()
        {
            try
            {
                IsLoading = true;

                bool result = await _backupService.RestoreDataAsync();
                if (result)
                {
                    DisplayInformation("Aby zastosować przywrócone ustawienia, aplikacja zostanie zamknięta.", "Wymagane ponowne uruchomienie");
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Błąd podczas przywracania danych");
            }
            finally
            {
                IsLoading = false;
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
                return DisplayQuestion(
                    "Masz niezapisane zmiany. Czy na pewno chcesz wyjść bez zapisywania?",
                    "Niezapisane zmiany");
            }

            return true;
        }
    }
}