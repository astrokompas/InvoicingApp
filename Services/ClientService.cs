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
        private readonly IDataStorage<Invoice> _invoiceStorage;

        public ClientService(
            IDataStorage<Client> clientStorage,
            IDataStorage<Invoice> invoiceStorage)
        {
            _clientStorage = clientStorage;
            _invoiceStorage = invoiceStorage;
        }

        public async Task<Client> GetClientWithInvoicesAsync(string id)
        {
            var client = await GetClientByIdAsync(id);
            if (client != null)
            {
                var invoices = await GetInvoicesByClientIdAsync(id);
                client.Invoices = invoices.ToList();
            }
            return client;
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

        private async Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(string clientId)
        {
            return await _invoiceStorage.QueryAsync(i => i.ClientId == clientId);
        }
    }
}