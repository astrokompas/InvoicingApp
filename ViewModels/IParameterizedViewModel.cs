using InvoicingApp.Models;
using InvoicingApp.Services;

namespace InvoicingApp.ViewModels
{
    public interface IParameterizedViewModel
    {
        /// <param name="parameter"></param>
        void ApplyParameter(NavigationParameter parameter);
    }
}