using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using InvoicingApp.Commands;
using InvoicingApp.Services;
using InvoicingApp.ViewModels;

namespace InvoicingApp
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private readonly IDialogService _dialogService;

        public MainWindow(IDialogService dialogService)
        {
            InitializeComponent();

            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            KeyDown += MainWindow_KeyDown;

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;
            MainWindowBorder.SizeChanged += MainWindowBorder_SizeChanged;
            UpdateClipping();

            Width = 1300;
            Height = 800;
            var workArea = SystemParameters.WorkArea;
            Left = (workArea.Width - Width) / 2;
            Top = (workArea.Height - Height) / 2;

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MainWindowViewModel;
            UpdateClipping();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Width = 1300;
                Height = 800;
                MainWindowBorder.Margin = new Thickness(0);
                var workArea = SystemParameters.WorkArea;
                Left = (workArea.Width - Width) / 2;
                Top = (workArea.Height - Height) / 2;
            }
            else if (WindowState == WindowState.Maximized)
            {
                var workArea = SystemParameters.WorkArea;
                Width = workArea.Width;
                Height = workArea.Height;
                Left = workArea.Left;
                Top = workArea.Top;
                double taskbarHeight = SystemParameters.PrimaryScreenHeight - workArea.Height;
                MainWindowBorder.Margin = new Thickness(8, 8, 8, 8 + taskbarHeight);
            }
            UpdateClipping();
        }

        private void MainWindowBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClipping();
        }

        private void UpdateClipping()
        {
            if (MainWindowBorder.ActualWidth > 0 && MainWindowBorder.ActualHeight > 0)
            {
                MainWindowBorder.Clip = new System.Windows.Media.RectangleGeometry
                {
                    RadiusX = 8,
                    RadiusY = 8,
                    Rect = new Rect(
                        0,
                        0,
                        MainWindowBorder.ActualWidth,
                        MainWindowBorder.ActualHeight
                    )
                };
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    ToggleWindowState();
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null)
            {
                _viewModel = DataContext as MainWindowViewModel;
                if (_viewModel == null) return;
            }

            // Navigation shortcuts
            if (KeyboardShortcutCommands.GoToInvoicesGesture.Matches(null, e))
            {
                _viewModel.NavigateToInvoicesCommand.Execute(null);
                e.Handled = true;
            }
            else if (KeyboardShortcutCommands.GoToClientsGesture.Matches(null, e))
            {
                _viewModel.NavigateToClientsCommand.Execute(null);
                e.Handled = true;
            }
            else if (KeyboardShortcutCommands.GoToReportsGesture.Matches(null, e))
            {
                _viewModel.NavigateToReportsCommand.Execute(null);
                e.Handled = true;
            }
            else if (KeyboardShortcutCommands.GoToSettingsGesture.Matches(null, e))
            {
                _viewModel.NavigateToSettingsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void ToggleWindowState()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool result = _dialogService.ShowQuestion("Czy na pewno chcesz zamknąć aplikację?", "Potwierdzenie zamknięcia");

            if (!result)
            {
                e.Cancel = true;
            }
        }
    }
}