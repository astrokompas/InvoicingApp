using System.Threading.Tasks;

namespace InvoicingApp.ViewModels
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}