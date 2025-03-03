using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using InvoicingApp.Pages;
using InvoicingApp.ViewModels;

namespace InvoicingApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _viewModelToViewMapping;
        private readonly Stack<NavigationHistoryEntry> _navigationHistory;

        public NavigationService(Frame frame, IServiceProvider serviceProvider)
        {
            _frame = frame;
            _serviceProvider = serviceProvider;
            _navigationHistory = new Stack<NavigationHistoryEntry>();

            // Register ViewModel -> View mappings
            _viewModelToViewMapping = new Dictionary<Type, Type>
            {
                { typeof(InvoiceListViewModel), typeof(InvoiceListPage) },
                { typeof(InvoiceEditorViewModel), typeof(InvoiceEditorPage) },
                { typeof(ClientsViewModel), typeof(ClientsPage) },
                { typeof(ReportsViewModel), typeof(ReportsPage) },
                { typeof(SettingsViewModel), typeof(SettingsPage) }
            };
        }

        public void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class
        {
            NavigateTo(typeof(TViewModel), parameter);
        }

        public void NavigateTo(Type viewModelType, NavigationParameter parameter = null)
        {
            if (!_viewModelToViewMapping.TryGetValue(viewModelType, out Type viewType))
            {
                throw new ArgumentException($"No view found for ViewModel type {viewModelType.Name}");
            }

            // Create the view instance
            var view = _serviceProvider.GetService(viewType) as Page;
            if (view == null)
            {
                throw new InvalidOperationException($"Failed to create view of type {viewType.Name}");
            }

            // Create and set the ViewModel
            var viewModel = _serviceProvider.GetService(viewModelType);
            if (viewModel == null)
            {
                throw new InvalidOperationException($"Failed to create ViewModel of type {viewModelType.Name}");
            }

            // Apply parameters to ViewModel if needed
            if (parameter != null && viewModel is IParameterizedViewModel paramViewModel)
            {
                paramViewModel.ApplyParameter(parameter);
            }

            view.DataContext = viewModel;

            // Add to navigation history
            _navigationHistory.Push(new NavigationHistoryEntry
            {
                ViewModelType = viewModelType,
                Parameter = parameter
            });

            // Navigate
            _frame.Navigate(view);
        }

        public bool CanGoBack => _navigationHistory.Count > 1;

        public void GoBack()
        {
            if (!CanGoBack)
            {
                return;
            }

            // Pop current page
            _navigationHistory.Pop();

            // Get previous page
            var previousEntry = _navigationHistory.Peek();

            // Navigate to it
            NavigateTo(previousEntry.ViewModelType, previousEntry.Parameter);
        }
    }

    public interface INavigationService
    {
        void NavigateTo<TViewModel>(NavigationParameter parameter = null) where TViewModel : class;
        void NavigateTo(Type viewModelType, NavigationParameter parameter = null);
        bool CanGoBack { get; }
        void GoBack();
    }

    public interface IParameterizedViewModel
    {
        void ApplyParameter(NavigationParameter parameter);
    }

    public class NavigationParameter
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public NavigationParameter() { }

        public NavigationParameter(string key, object value)
        {
            Add(key, value);
        }

        public void Add(string key, object value)
        {
            _parameters[key] = value;
        }

        public T Get<T>(string key)
        {
            if (_parameters.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        public bool Contains(string key)
        {
            return _parameters.ContainsKey(key);
        }
    }

    public class NavigationHistoryEntry
    {
        public Type ViewModelType { get; set; }
        public NavigationParameter Parameter { get; set; }
    }
}