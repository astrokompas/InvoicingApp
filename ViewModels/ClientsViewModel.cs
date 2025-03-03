using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public class ClientsViewModel : INotifyPropertyChanged
    {
        private readonly IClientService _clientService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Client> _clients;
        private string _searchText;
        private ICollectionView _filteredClients;
        private Client _selectedClient;
        private bool _isLoading;

        // For adding/editing clients
        private Client _currentClient = new Client();
        private bool _isEditing;

        public ClientsViewModel(IClientService clientService, INavigationService navigationService)
        {
            _clientService = clientService;
            _navigationService = navigationService;

            // Initialize commands
            AddClientCommand = new RelayCommand(AddClient, CanAddClient);
            EditClientCommand = new RelayCommand(EditClient, CanEditClient);
            DeleteClientCommand = new RelayCommand(DeleteClient, CanDeleteClient);
            SaveClientCommand = new RelayCommand(SaveClient, CanSaveClient);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            RefreshCommand = new RelayCommand(Refresh);

            // Load clients
            LoadClientsAsync();
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                _filteredClients = CollectionViewSource.GetDefaultView(_clients);
                _filteredClients.Filter = FilterClients;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredClients));
            }
        }

        public ICollectionView FilteredClients => _filteredClients;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _filteredClients?.Refresh();
                OnPropertyChanged();
            }
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Client CurrentClient
        {
            get => _currentClient;
            set
            {
                _currentClient = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
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

        // Statistics
        public int TotalClientCount => Clients?.Count ?? 0;
        public int ActiveClientCount => Clients?.Count(c => c.IsActive) ?? 0;

        // Commands
        public ICommand AddClientCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand SaveClientCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void LoadClientsAsync()
        {
            IsLoading = true;

            try
            {
                var clients = await _clientService.GetAllClientsAsync();

                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                // Handle error (log or show message)
                System.Windows.MessageBox.Show($"Błąd podczas ładowania klientów: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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
                // Create a copy for editing
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
                // Confirm deletion
                var result = System.Windows.MessageBox.Show(
                    $"Czy na pewno chcesz usunąć klienta {SelectedClient.Name}?",
                    "Potwierdzenie usunięcia",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        await _clientService.DeleteClientAsync(SelectedClient.Id);
                        Clients.Remove(SelectedClient);
                    }
                    catch (Exception ex)
                    {
                        // Handle error
                        System.Windows.MessageBox.Show($"Błąd podczas usuwania klienta: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                    // Validate client data
                    if (string.IsNullOrWhiteSpace(CurrentClient.Name))
                    {
                        System.Windows.MessageBox.Show("Nazwa klienta jest wymagana.", "Błąd walidacji", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(CurrentClient.TaxID))
                    {
                        System.Windows.MessageBox.Show("NIP klienta jest wymagany.", "Błąd walidacji", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    // Save client
                    await _clientService.SaveClientAsync(CurrentClient);

                    // If it's a new client, add it to the collection
                    if (string.IsNullOrEmpty(CurrentClient.Id))
                    {
                        Clients.Add(CurrentClient);
                    }
                    else
                    {
                        // Update the existing client in the collection
                        int index = Clients.IndexOf(SelectedClient);
                        if (index >= 0)
                        {
                            Clients[index] = CurrentClient;
                        }
                    }

                    IsEditing = false;
                    CurrentClient = new Client();
                }
                catch (Exception ex)
                {
                    // Handle error
                    System.Windows.MessageBox.Show($"Błąd podczas zapisywania klienta: {ex.Message}", "Błąd", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
            LoadClientsAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}