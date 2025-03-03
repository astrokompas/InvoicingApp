using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using InvoicingApp.Services;
using InvoicingApp.ViewModels;
using InvoicingApp.Pages;
using InvoicingApp.DataStorage;
using InvoicingApp.Models;
using InvoicingApp.Converters;

namespace InvoicingApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
            Resources.Add("BoolToVisibilityInverseConverter", new BoolToVisibilityInverseConverter());
            Resources.Add("StringEqualsConverter", new StringEqualsConverter());
            Resources.Add("StringEqualsToVisibilityConverter", new StringEqualsToVisibilityConverter());
            Resources.Add("StringNotEqualsToVisibilityConverter", new StringNotEqualsToVisibilityConverter());
            Resources.Add("StringEmptyToVisibilityConverter", new StringEmptyToVisibilityConverter());
            Resources.Add("StringNotEmptyToVisibilityConverter", new StringNotEmptyToVisibilityConverter());
            Resources.Add("StatusToColorConverter", new StatusToColorConverter());
            Resources.Add("BoolToBackgroundConverter", new BoolToBackgroundConverter());
            Resources.Add("NullToAddEditClientConverter", new NullToAddEditClientConverter());
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "faktury-MOVING"
            );

            // Ensure directories exist
            EnsureDirectoryExists(Path.Combine(appDataPath, "Invoices"));
            EnsureDirectoryExists(Path.Combine(appDataPath, "Clients"));

            // Data Storage
            services.AddSingleton<IDataStorage<Invoice>>(
                new JsonStorage<Invoice>(Path.Combine(appDataPath, "Invoices")));
            services.AddSingleton<IDataStorage<Client>>(
                new JsonStorage<Client>(Path.Combine(appDataPath, "Clients")));
            services.AddSingleton<IDataStorage<AppSettings>>(
                new JsonStorage<AppSettings>(appDataPath));

            // Services
            services.AddSingleton<IInvoiceService, InvoiceService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IPDFService, PDFService>();
            services.AddSingleton<IReportService, ReportService>();

            // Register views
            services.AddTransient<MainWindow>();
            services.AddTransient<InvoiceListPage>();
            services.AddTransient<InvoiceEditorPage>();
            services.AddTransient<ClientsPage>();
            services.AddTransient<ReportsPage>();
            services.AddTransient<SettingsPage>();

            // Register view models
            services.AddTransient<InvoiceListViewModel>();
            services.AddTransient<InvoiceEditorViewModel>();
            services.AddTransient<ClientsViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Navigation (will be initialized in MainWindow after Frame is created)
            services.AddSingleton<Func<INavigationService>>(provider => () =>
            {
                var mainWindow = provider.GetRequiredService<MainWindow>();
                return new NavigationService(mainWindow.MainFrame, provider);
            });
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Initialize navigation service
            var navigationServiceFactory = _serviceProvider.GetRequiredService<Func<INavigationService>>();
            var navigationService = navigationServiceFactory();

            // Navigate to initial view
            navigationService.NavigateTo<InvoiceListViewModel>();
        }
    }
}