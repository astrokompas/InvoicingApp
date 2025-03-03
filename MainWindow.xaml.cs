using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace InvoicingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Borderless window setup
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            Opacity = 1;
            Visibility = Visibility.Visible;
            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;
            MainWindowBorder.SizeChanged += MainWindowBorder_SizeChanged;
            UpdateClipping();

            // Set initial size and position
            Width = 1300;
            Height = 800;

            var workArea = SystemParameters.WorkArea;
            Left = (workArea.Width - Width) / 2;
            Top = (workArea.Height - Height) / 2;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
            MainWindowBorder.Clip = new RectangleGeometry
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
    }
}