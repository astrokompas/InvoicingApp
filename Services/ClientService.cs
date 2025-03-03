using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvoicingApp.DataStorage;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public class ClientService : IClientService
    {
        private readonly IDataStorage<Client> _clientStorage;

        public ClientService(IDataStorage<Client> clientStorage)
        {
            _clientStorage = clientStorage;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _clientStorage.GetAllAsync();
        }

        public async Task<Client> GetClientByIdAsync(string id)
        {
            return await _clientStorage.GetByIdAsync(id);
        }

        public async Task SaveClientAsync(Client client)
        {
            await _clientStorage.SaveAsync(client);
        }

        public async Task DeleteClientAsync(string id)
        {
            await _clientStorage.DeleteAsync(id);
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchText)
        {
            return await _clientStorage.QueryAsync(c =>
                c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                c.TaxID.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}