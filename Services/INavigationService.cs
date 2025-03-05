using System;
using System.Threading.Tasks;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Navigate to a view associated with the specified ViewModel type.
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel to navigate to</typeparam>
        /// <param name="parameter">Optional navigation parameters</param>
        void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class;
        void NavigateTo(Type viewModelType, NavigationParameter parameter = null);

        /// <summary>
        /// Navigate to a view associated with the specified ViewModel type.
        /// </summary>
        /// <param name="viewModelType">The type of ViewModel to navigate to</param>
        /// <param name="parameter">Optional navigation parameters</param>
        Task NavigateToAsync<TViewModel>(NavigationParameter parameter = null) where TViewModel : class;
        Task NavigateToAsync(Type viewModelType, NavigationParameter parameter = null);

        /// <summary>
        /// Gets a value indicating whether navigation back is possible
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Navigate back to the previous view
        /// </summary>
        void GoBack();

        /// <summary>
        /// Gets the current ViewModel
        /// </summary>
        object CurrentViewModel { get; }
    }
}