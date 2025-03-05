using System.Collections.Generic;
using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> GetClientByIdAsync(string id);
        Task SaveClientAsync(Client client);
        Task DeleteClientAsync(string id);
        Task<IEnumerable<Client>> SearchClientsAsync(string searchText);
        Task<Client> GetClientWithInvoicesAsync(string id);
    }
}