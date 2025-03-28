# InvoicingApp

A comprehensive WPF invoicing application for small businesses built with modern .NET technologies. Invoicing App provides a sleek, user-friendly interface for managing invoices, clients, payments, and generating detailed reports.

## 📋 Features

### Invoice Management
- Create, edit, and delete invoices
- Customizable invoice numbering system
- Add multiple items with different VAT rates
- Track payment status (Paid, Partially Paid, Unpaid, Overdue)
- Record partial payments
- Export invoices to PDF format

### Client Management
- Comprehensive client database
- Store client contact details, tax IDs, and addresses
- View client payment history
- Active/inactive client status tracking

### Financial Reporting
- Generate detailed sales reports
- Filter reports by date range and client
- View key performance indicators like total invoices, paid amounts, and outstanding balances
- Export reports to PDF format

### Settings and Customization
- Customize company details and logo
- Configure default payment terms
- Manage VAT rates and currencies
- Create and restore data backups
- Data retention settings

## 🏗️ Architecture

The application follows the **MVVM (Model-View-ViewModel)** architecture pattern with dependency injection:

- **Models:** Represent the data and business logic
- **Views:** XAML-based UI components
- **ViewModels:** Connect the models to the views, handle user interactions
- **Services:** Core business logic and data operations

### Design Patterns Used
- **MVVM:** Clean separation of UI and business logic
- **Repository Pattern:** For data storage abstraction
- **Dependency Injection:** For loose coupling of components
- **Command Pattern:** For UI interactions
- **Factory Pattern:** For service instantiation

## 🧰 Technologies Used

- **Framework:** .NET 8.0 with WPF
- **Language:** C# 12
- **Data Storage:** JSON-based file storage
- **Styling:** Custom XAML styles and templates
- **PDF Generation:** PDFSharp
- **UI:** Custom-designed modern interface with dark theme
- **Build System:** MSBuild

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK or newer
- Visual Studio 2022 (recommended)

## 💻 Usage

### Invoice Management
- Navigate to the "Faktury" (Invoices) section.
- Click "Nowa faktura" to create a new invoice.
- Fill in client details, add invoice items, and set payment terms.
- Save the invoice using `Ctrl+S` or the Save button.
- Export to PDF with the "Eksportuj PDF" button.

### Client Management
- Navigate to the "Klienci" (Clients) section.
- Add new clients with the "Dodaj klienta" button.
- Edit existing clients by selecting them and clicking the edit icon.
- View client history and manage active/inactive status.

### Reports Generation
- Navigate to the "Raporty" (Reports) section.
- Set the date range for your report.
- Optionally select a specific client.
- Click "Odśwież" to generate the report.
- Export to PDF with the "Eksportuj PDF" button.

### System Settings
- Navigate to the "Ustawienia" (Settings) section.
- Configure company details, including logo.
- Set up VAT rates and currencies.
- Configure backup options.
- Manage data retention policies.

## 📂 Project Structure

```bash
InvoicingApp/
├── Commands/              # Command implementations for MVVM
├── Converters/            # Value converters for XAML bindings
├── DataStorage/           # Storage implementations and interfaces
├── Models/                # Domain models and data structures
├── Pages/                 # XAML pages for each section
├── Services/              # Business logic services
│   ├── BackupService.cs   # Backup and restore functionality
│   ├── ClientService.cs   # Client management
│   ├── InvoiceService.cs  # Invoice operations
│   ├── PDFService.cs      # PDF generation
│   ├── ReportService.cs   # Reporting logic
│   └── SettingsService.cs # Application settings
├── ViewModels/            # ViewModels for MVVM pattern
└── App.xaml/.cs           # Application entry point
```

## 🔧 Development

### Keyboard Shortcuts
- `Ctrl+N`: Create new invoice/client
- `Ctrl+S`: Save current item
- `Ctrl+P`: Export to PDF
- `F5`: Refresh data
- `Ctrl+1/2/3`: Switch between invoice editor modes
- `Esc`: Cancel current operation

### Navigation
- Navigation between main sections is handled via the sidebar.
- Each section (Invoices, Clients, Reports, Settings) has its own page.
- Modal dialogs are used for operations like adding payments.
