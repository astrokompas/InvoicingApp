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

            // Register dialog service first since it's used in many places
            services.AddSingleton<IDialogService, DialogService>();

            // Data Storage
            services.AddSingleton<IDataStorage<Invoice>>(
                new JsonStorage<Invoice>(Path.Combine(appDataPath, "Invoices")));
            services.AddSingleton<IDataStorage<Client>>(
                new JsonStorage<Client>(Path.Combine(appDataPath, "Clients")));
            services.AddSingleton<IDataStorage<AppSettings>>(
                new JsonStorage<AppSettings>(appDataPath));

            // Services
            services.AddSingleton<ISettingsService, SettingsService>();

            // First register with interfaces to avoid circular dependencies
            services.AddSingleton<IInvoiceService, InvoiceService>();
            services.AddSingleton<IClientService, ClientService>();

            // Fix services with dependencies
            services.AddSingleton<IBackupService>(sp =>
                new BackupService(
                    appDataPath,
                    sp.GetRequiredService<IDialogService>()
                )
            );
            services.AddSingleton<IPDFService, PDFService>();
            services.AddSingleton<IReportService, ReportService>();
            services.AddSingleton<ReportPDFService>();

            // Register views
            services.AddTransient<MainWindow>();
            services.AddTransient<InvoiceListPage>();
            services.AddTransient<InvoiceEditorPage>();
            services.AddTransient<ClientsPage>();
            services.AddTransient<ReportsPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<AddPaymentPage>();

            // Register view models
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InvoiceListViewModel>();
            services.AddTransient<InvoiceEditorViewModel>();
            services.AddTransient<ClientsViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AddPaymentViewModel>();

            // Navigation Service
            services.AddSingleton<INavigationService>(provider =>
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

            // Initialize MainWindow
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.Show();

            // Get NavigationService
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();

            Task.Run(async () =>
            {
                // Navigate to initial view and wait for initialization
                await navigationService.NavigateToAsync<InvoiceListViewModel>();

                // Access the UI thread to update the active view
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (mainWindow.DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.ActiveView = "Invoices";
                    }
                });
            });
        }
    }
}