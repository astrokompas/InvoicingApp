using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using InvoicingApp.Pages;
using InvoicingApp.ViewModels;
using InvoicingApp.Models;
using System.Diagnostics;

namespace InvoicingApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _viewModelToViewMapping;
        private readonly Stack<NavigationHistoryEntry> _navigationHistory;

        public object CurrentViewModel { get; private set; }

        public NavigationService(Frame frame, IServiceProvider serviceProvider)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _navigationHistory = new Stack<NavigationHistoryEntry>();

            // Register ViewModel -> View mappings
            _viewModelToViewMapping = new Dictionary<Type, Type>
            {
                { typeof(InvoiceListViewModel), typeof(InvoiceListPage) },
                { typeof(InvoiceEditorViewModel), typeof(InvoiceEditorPage) },
                { typeof(ClientsViewModel), typeof(ClientsPage) },
                { typeof(ReportsViewModel), typeof(ReportsPage) },
                { typeof(SettingsViewModel), typeof(SettingsPage) },
                { typeof(AddPaymentViewModel), typeof(AddPaymentPage) }
            };
        }

        // Synchronous navigation methods
        public void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class
        {
            NavigateTo(typeof(TViewModel), parameter);
        }

        public void NavigateTo(Type viewModelType, NavigationParameter parameter = null)
        {
            _ = NavigateToAsyncInternal(viewModelType, parameter);
        }

        // Asynchronous navigation methods
        public Task NavigateToAsync<TViewModel>(NavigationParameter parameter = null) where TViewModel : class
        {
            return NavigateToAsync(typeof(TViewModel), parameter);
        }

        public Task NavigateToAsync(Type viewModelType, NavigationParameter parameter = null)
        {
            return NavigateToAsyncInternal(viewModelType, parameter);
        }

        private async Task NavigateToAsyncInternal(Type viewModelType, NavigationParameter parameter = null)
        {
            try
            {
                if (!_viewModelToViewMapping.TryGetValue(viewModelType, out Type viewType))
                {
                    throw new ArgumentException($"No view found for ViewModel type {viewModelType.Name}");
                }

                // Create view and view model
                Page view = _serviceProvider.GetService(viewType) as Page;
                BaseViewModel viewModel = _serviceProvider.GetService(viewModelType) as BaseViewModel;

                if (view == null)
                {
                    throw new InvalidOperationException($"Failed to create view of type {viewType.Name}");
                }

                if (viewModel == null)
                {
                    throw new InvalidOperationException($"Failed to create ViewModel of type {viewModelType.Name}");
                }

                // Store current view model
                CurrentViewModel = viewModel;

                // Apply parameters
                if (parameter != null && viewModel is IParameterizedViewModel paramViewModel)
                {
                    paramViewModel.ApplyParameter(parameter);
                }

                // Set data context
                view.DataContext = viewModel;

                // Add to navigation history
                _navigationHistory.Push(new NavigationHistoryEntry
                {
                    ViewModelType = viewModelType,
                    Parameter = parameter
                });

                // Navigate to view
                _frame.Navigate(view);

                // Initialize async view model
                if (viewModel is IAsyncInitializable asyncViewModel)
                {
                    await asyncViewModel.InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                throw;
            }
        }

        public bool CanGoBack => _navigationHistory.Count > 1;

        public void GoBack()
        {
            if (!CanGoBack)
                return;

            // Pop current page
            _navigationHistory.Pop();

            // Get previous page
            var previousEntry = _navigationHistory.Peek();

            // Navigate to previous page
            NavigateTo(previousEntry.ViewModelType, previousEntry.Parameter);
        }
    }
}