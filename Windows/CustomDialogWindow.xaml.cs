using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace InvoicingApp.Windows
{
    public enum DialogType
    {
        Information,
        Success,
        Warning,
        Error,
        Question
    }

    public partial class CustomDialogWindow : Window
    {
        public bool Result { get; private set; } = false;

        public CustomDialogWindow(Window owner, string message, string title, DialogType type)
        {
            InitializeComponent();

            Owner = owner;
            TitleText.Text = title;
            MessageText.Text = message;

            // Configure based on dialog type
            ConfigureDialogType(type);

            // Set custom icon if provided
            SetIconForType(type);

            // Set keyboard shortcuts
            KeyDown += CustomDialogWindow_KeyDown;
        }

        private void ConfigureDialogType(DialogType type)
        {
            switch (type)
            {
                case DialogType.Question:
                    // Show Yes/No buttons for questions
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    break;
                default:
                    // Show just OK button for other types
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetIconForType(DialogType type)
        {
            switch (type)
            {
                case DialogType.Information:
                    IconText.Text = "ℹ";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E6CB2"));
                    break;
                case DialogType.Success:
                    IconText.Text = "✓";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                    break;
                case DialogType.Warning:
                    IconText.Text = "⚠";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                    break;
                case DialogType.Error:
                    IconText.Text = "❌";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E22B2B"));
                    break;
                case DialogType.Question:
                    IconText.Text = "?";
                    IconText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E6CB2"));
                    break;
            }
        }

        private void CustomDialogWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Result = false;
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                if (OkButton.Visibility == Visibility.Visible)
                {
                    Result = true;
                    DialogResult = true;
                    Close();
                }
                else if (YesButton.Visibility == Visibility.Visible)
                {
                    Result = true;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
            Close();
        }

        // Static helper methods to show dialogs
        public static bool ShowQuestion(Window owner, string message, string title)
        {
            var dialog = new CustomDialogWindow(owner, message, title, DialogType.Question);
            dialog.ShowDialog();
            return dialog.Result;
        }

        public static void ShowInformation(Window owner, string message, string title)
        {
            var dialog = new CustomDialogWindow(owner, message, title, DialogType.Information);
            dialog.ShowDialog();
        }

        public static void ShowWarning(Window owner, string message, string title)
        {
            var dialog = new CustomDialogWindow(owner, message, title, DialogType.Warning);
            dialog.ShowDialog();
        }

        public static void ShowError(Window owner, string message, string title)
        {
            var dialog = new CustomDialogWindow(owner, message, title, DialogType.Error);
            dialog.ShowDialog();
        }

        public static void ShowSuccess(Window owner, string message, string title)
        {
            var dialog = new CustomDialogWindow(owner, message, title, DialogType.Success);
            dialog.ShowDialog();
        }
    }
}