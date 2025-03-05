using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using InvoicingApp.Pages;
using InvoicingApp.ViewModels;
using InvoicingApp.Models;

namespace InvoicingApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _viewModelToViewMapping;
        private readonly Stack<NavigationHistoryEntry> _navigationHistory;

        public object CurrentViewModel { get; private set; }

        public NavigationService(Frame frame)
        {
            _frame = frame;
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

        public NavigationService(Frame frame, IServiceProvider serviceProvider) : this(frame)
        {
            _serviceProvider = serviceProvider;
        }

        // Synchronous navigation methods
        public void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class
        {
            NavigateTo(typeof(TViewModel), parameter);
        }

        public void NavigateTo(Type viewModelType, NavigationParameter parameter = null)
        {
            // Call async method but don't await it
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
            if (!_viewModelToViewMapping.TryGetValue(viewModelType, out Type viewType))
            {
                throw new ArgumentException($"No view found for ViewModel type {viewModelType.Name}");
            }

            // Create view and view model
            Page view;
            BaseViewModel viewModel;

            if (_serviceProvider != null)
            {
                // Use dependency injection
                view = _serviceProvider.GetService(viewType) as Page;
                viewModel = _serviceProvider.GetService(viewModelType) as BaseViewModel;
            }
            else
            {
                // Create directly
                view = Activator.CreateInstance(viewType) as Page;
                // Simple activation without dependencies - this would need more logic
                viewModel = Activator.CreateInstance(viewModelType) as BaseViewModel;
            }

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

            // Apply parameters if needed
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

            // Initialize async view model if needed
            if (viewModel is IAsyncInitializable asyncViewModel)
            {
                await asyncViewModel.InitializeAsync();
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