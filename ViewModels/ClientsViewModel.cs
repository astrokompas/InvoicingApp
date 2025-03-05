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
    public class ClientsViewModel : BaseViewModel, IAsyncInitializable
    {
        private readonly IClientService _clientService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Client> _clients;
        private string _searchText;
        private ICollectionView _filteredClients;
        private Client _selectedClient;

        private Client _currentClient = new Client();
        private bool _isEditing;
        private bool _hasClients;

        public ClientsViewModel(
            IClientService clientService,
            INavigationService navigationService,
            IDialogService dialogService)
            : base(dialogService)
        {
            _clientService = clientService;
            _navigationService = navigationService;

            AddClientCommand = new RelayCommand(AddClient, CanAddClient);
            EditClientCommand = new RelayCommand(EditClient, CanEditClient);
            DeleteClientCommand = new RelayCommand(DeleteClient, CanDeleteClient);
            SaveClientCommand = new RelayCommand(SaveClient, CanSaveClient);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            RefreshCommand = new RelayCommand(Refresh);

            Clients = new ObservableCollection<Client>();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadClientsAsync();
            }
            catch (Exception ex)
            {
                DisplayError(ex, "Failed to initialize");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                if (SetProperty(ref _clients, value))
                {
                    _filteredClients = CollectionViewSource.GetDefaultView(_clients);
                    _filteredClients.Filter = FilterClients;
                    OnPropertyChanged(nameof(FilteredClients));
                    OnPropertyChanged(nameof(TotalClientCount));
                    OnPropertyChanged(nameof(ActiveClientCount));
                    HasClients = _clients.Count > 0;
                }
            }
        }

        public ICollectionView FilteredClients => _filteredClients;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _filteredClients?.Refresh();
            }
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetProperty(ref _selectedClient, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public Client CurrentClient
        {
            get => _currentClient;
            set => SetProperty(ref _currentClient, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        public bool HasClients
        {
            get => _hasClients;
            set => SetProperty(ref _hasClients, value);
        }

        public int TotalClientCount => Clients?.Count ?? 0;
        public int ActiveClientCount => Clients?.Count(c => c.IsActive) ?? 0;

        public ICommand AddClientCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand SaveClientCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadClientsAsync()
        {
            var clients = await _clientService.GetAllClientsAsync();
            Clients = new ObservableCollection<Client>(clients);
        }

        private bool FilterClients(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (item is Client client)
            {
                return client.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       client.TaxID.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       (client.ContactPerson != null && client.ContactPerson.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        private void AddClient()
        {
            CurrentClient = new Client { IsActive = true };
            IsEditing = true;
        }

        private bool CanAddClient()
        {
            return !IsEditing;
        }

        private void EditClient()
        {
            if (SelectedClient != null)
            {
                CurrentClient = new Client
                {
                    Id = SelectedClient.Id,
                    Name = SelectedClient.Name,
                    Address = SelectedClient.Address,
                    TaxID = SelectedClient.TaxID,
                    ContactPerson = SelectedClient.ContactPerson,
                    Email = SelectedClient.Email,
                    Phone = SelectedClient.Phone,
                    IsActive = SelectedClient.IsActive
                };

                IsEditing = true;
            }
        }

        private bool CanEditClient()
        {
            return SelectedClient != null && !IsEditing;
        }

        private async void DeleteClient()
        {
            if (SelectedClient != null)
            {
                if (DisplayQuestion($"Czy na pewno chcesz usunąć klienta {SelectedClient.Name}?",
                    "Potwierdzenie usunięcia"))
                {
                    try
                    {
                        IsLoading = true;
                        await _clientService.DeleteClientAsync(SelectedClient.Id);
                        Clients.Remove(SelectedClient);
                        DisplayInformation("Klient został usunięty pomyślnie.", "Usunięto");
                    }
                    catch (Exception ex)
                    {
                        DisplayError(ex, "Błąd podczas usuwania klienta");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private bool CanDeleteClient()
        {
            return SelectedClient != null && !IsEditing;
        }

        private async void SaveClient()
        {
            if (CurrentClient != null)
            {
                try
                {
                    IsLoading = true;

                    if (string.IsNullOrWhiteSpace(CurrentClient.Name))
                    {
                        DisplayWarning("Nazwa klienta jest wymagana.", "Błąd walidacji");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(CurrentClient.TaxID))
                    {
                        DisplayWarning("NIP klienta jest wymagany.", "Błąd walidacji");
                        return;
                    }

                    await _clientService.SaveClientAsync(CurrentClient);

                    if (string.IsNullOrEmpty(CurrentClient.Id))
                    {
                        Clients.Add(CurrentClient);
                    }
                    else
                    {
                        int index = Clients.IndexOf(SelectedClient);
                        if (index >= 0)
                        {
                            Clients[index] = CurrentClient;
                        }
                    }

                    IsEditing = false;
                    CurrentClient = new Client();

                    DisplayInformation("Klient został zapisany pomyślnie.", "Zapisano");
                }
                catch (Exception ex)
                {
                    DisplayError(ex, "Błąd podczas zapisywania klienta");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanSaveClient()
        {
            return IsEditing;
        }

        private void CancelEdit()
        {
            IsEditing = false;
            CurrentClient = new Client();
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        private void Refresh()
        {
            _ = InitializeAsync();
        }
    }
}