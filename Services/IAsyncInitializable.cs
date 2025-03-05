using System.Threading.Tasks;

namespace InvoicingApp.Services
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}