using System;
using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface INavigationService
    {
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="parameter"></param>
        void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class;
        void NavigateTo(Type viewModelType, NavigationParameter parameter = null);

        /// <param name="viewModelType"></param>
        /// <param name="parameter"></param>
        Task NavigateToAsync<TViewModel>(NavigationParameter parameter = null) where TViewModel : class;
        Task NavigateToAsync(Type viewModelType, NavigationParameter parameter = null);

        // Gets a value indicating whether navigation back is possible
        bool CanGoBack { get; }

        // Navigate back to the previous view
        void GoBack();

        // Gets the current ViewModel
        object CurrentViewModel { get; }
    }
}