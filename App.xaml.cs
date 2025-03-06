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
        private MainWindow _mainWindow;

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

            services.AddSingleton<IDialogService, DialogService>();

            // Data Storage
            services.AddSingleton<IDataStorage<Invoice>>(
                new JsonStorage<Invoice>(Path.Combine(appDataPath, "Invoices")));
            services.AddSingleton<IDataStorage<Client>>(
                new JsonStorage<Client>(Path.Combine(appDataPath, "Clients")));
            services.AddSingleton<IDataStorage<AppSettings>>(
                new JsonStorage<AppSettings>(appDataPath));

            // Core Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IInvoiceService, InvoiceService>();
            services.AddSingleton<IReportService, ReportService>();
            services.AddSingleton<IPDFService, PDFService>();

            services.AddSingleton<MainWindow>();

            services.AddSingleton<INavigationService>(provider => {
                var mainWindow = provider.GetRequiredService<MainWindow>();
                return new NavigationService(mainWindow.MainFrame, provider);
            });

            // Additional Services
            services.AddSingleton<IBackupService>(sp =>
                new BackupService(
                    appDataPath,
                    sp.GetRequiredService<IDialogService>()
                )
            );
            services.AddSingleton<ReportPDFService>();

            // View Models
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InvoiceListViewModel>();
            services.AddTransient<InvoiceEditorViewModel>();
            services.AddTransient<ClientsViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AddPaymentViewModel>();

            // Views
            services.AddTransient<InvoiceListPage>();
            services.AddTransient<InvoiceEditorPage>();
            services.AddTransient<ClientsPage>();
            services.AddTransient<ReportsPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<AddPaymentPage>();
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
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            mainWindow.DataContext = mainWindowViewModel;
            mainWindow.Show();

            navigationService.NavigateToAsync<InvoiceListViewModel>().ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindowViewModel.ActiveView = "Invoices";
                });
            });
        }
    }
}