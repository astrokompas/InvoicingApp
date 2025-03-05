using System;
using System.Threading.Tasks;
using InvoicingApp.Models;
using InvoicingApp.ViewModels;

namespace InvoicingApp.Services
{
    public static class NavigationExtensions
    {
        public static async Task NavigateToAsync<TViewModel>(this INavigationService navigationService, NavigationParameter parameter = null)
            where TViewModel : class
        {
            // First navigate to the view
            navigationService.NavigateTo<TViewModel>(parameter);

            // Then check if we need to await initialization
            if (navigationService.CurrentViewModel is IAsyncInitializable asyncViewModel)
            {
                await asyncViewModel.InitializeAsync();
            }
        }

        public static async Task NavigateToAsync(this INavigationService navigationService, Type viewModelType, NavigationParameter parameter = null)
        {
            // First navigate to the view
            navigationService.NavigateTo(viewModelType, parameter);

            // Then check if we need to await initialization
            if (navigationService.CurrentViewModel is IAsyncInitializable asyncViewModel)
            {
                await asyncViewModel.InitializeAsync();
            }
        }

        public static async Task GoBackAsync(this INavigationService navigationService)
        {
            if (!navigationService.CanGoBack)
                return;

            // Go back to the previous view
            navigationService.GoBack();

            // Then check if we need to await initialization
            if (navigationService.CurrentViewModel is IAsyncInitializable asyncViewModel)
            {
                await asyncViewModel.InitializeAsync();
            }
        }
    }
}