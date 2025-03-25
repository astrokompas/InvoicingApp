using System.Windows.Controls;
using InvoicingApp.ViewModels;

namespace InvoicingApp.Pages
{
    public partial class InvoiceEditorPage : Page
    {
        public InvoiceEditorPage()
        {
            InitializeComponent();
        }

        private void PaymentDeadline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is InvoiceEditorViewModel viewModel)
            {
                viewModel.OnPaymentDeadlineSelectionChanged(sender, e);
            }
        }
    }
}