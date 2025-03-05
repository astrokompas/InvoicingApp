using System.Windows.Input;

namespace InvoicingApp.Commands
{
    public static class KeyboardShortcutCommands
    {
        // Main navigation shortcuts
        public static readonly KeyGesture GoToInvoicesGesture = new KeyGesture(Key.F, ModifierKeys.Control);
        public static readonly KeyGesture GoToClientsGesture = new KeyGesture(Key.C, ModifierKeys.Control);
        public static readonly KeyGesture GoToReportsGesture = new KeyGesture(Key.R, ModifierKeys.Control);
        public static readonly KeyGesture GoToSettingsGesture = new KeyGesture(Key.S, ModifierKeys.Control);

        // Common actions
        public static readonly KeyGesture NewInvoiceGesture = new KeyGesture(Key.N, ModifierKeys.Control);
        public static readonly KeyGesture SaveGesture = new KeyGesture(Key.S, ModifierKeys.Control);
        public static readonly KeyGesture RefreshGesture = new KeyGesture(Key.F5);
        public static readonly KeyGesture PrintGesture = new KeyGesture(Key.P, ModifierKeys.Control);
        public static readonly KeyGesture ExportPdfGesture = new KeyGesture(Key.E, ModifierKeys.Control);
        public static readonly KeyGesture SearchGesture = new KeyGesture(Key.F, ModifierKeys.Control);

        // Navigation within app
        public static readonly KeyGesture BackGesture = new KeyGesture(Key.Escape);
    }
}