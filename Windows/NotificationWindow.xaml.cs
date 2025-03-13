using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace InvoicingApp.Windows
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer _closeTimer;
        private static readonly Dictionary<Window, List<NotificationWindow>> _notificationWindows =
            new Dictionary<Window, List<NotificationWindow>>();
        private Window _ownerWindow;

        public NotificationWindow(Window owner, string message, string title, string icon, Color backgroundColor, int durationMs)
        {
            InitializeComponent();

            _ownerWindow = owner;

            // Register notification
            if (!_notificationWindows.ContainsKey(owner))
            {
                _notificationWindows[owner] = new List<NotificationWindow>();
                // Add handler to update positions when owner moves or resizes
                owner.LocationChanged += OwnerWindow_LocationChanged;
                owner.SizeChanged += OwnerWindow_SizeChanged;
            }
            _notificationWindows[owner].Add(this);

            TitleText.Text = title;
            MessageText.Text = message;
            IconText.Text = icon;
            MainBorder.Background = new SolidColorBrush(backgroundColor);

            // Add close timer
            if (durationMs > 0)
            {
                _closeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(durationMs)
                };

                _closeTimer.Tick += (s, e) =>
                {
                    _closeTimer.Stop();
                    CloseWithAnimation();
                };
            }

            Loaded += NotificationWindow_Loaded;
            Closed += NotificationWindow_Closed;

            Owner = owner;
            ShowInTaskbar = false;
        }

        private void OwnerWindow_LocationChanged(object sender, EventArgs e)
        {
            UpdateAllPositions(_ownerWindow);
        }

        private void OwnerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAllPositions(_ownerWindow);
        }

        private static void UpdateAllPositions(Window owner)
        {
            if (!_notificationWindows.ContainsKey(owner))
                return;

            var notifications = _notificationWindows[owner];
            double topOffset = 10;

            foreach (var notification in notifications)
            {
                notification.UpdatePosition(topOffset);
                topOffset += notification.ActualHeight + 10;
            }
        }

        private void UpdatePosition(double topOffset)
        {
            // Get position in owner window coordinates
            double rightMargin = 20;
            double ownerWidth = _ownerWindow.ActualWidth;

            // Calculate position relative to owner
            Left = _ownerWindow.Left + ownerWidth - Width - rightMargin;
            Top = _ownerWindow.Top + topOffset;
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Position window initially
            UpdateAllPositions(_ownerWindow);

            // Animate window
            Opacity = 0;

            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            BeginAnimation(OpacityProperty, fadeInAnimation);

            // Start close timer if exists
            _closeTimer?.Start();
        }

        private void NotificationWindow_Closed(object sender, EventArgs e)
        {
            if (_notificationWindows.ContainsKey(_ownerWindow))
            {
                _notificationWindows[_ownerWindow].Remove(this);

                // Update positions of remaining notifications
                UpdateAllPositions(_ownerWindow);

                // If no more notifications, remove owner from dictionary
                if (_notificationWindows[_ownerWindow].Count == 0)
                {
                    _ownerWindow.LocationChanged -= OwnerWindow_LocationChanged;
                    _ownerWindow.SizeChanged -= OwnerWindow_SizeChanged;
                    _notificationWindows.Remove(_ownerWindow);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAnimation();
        }

        private void CloseWithAnimation()
        {
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            fadeOutAnimation.Completed += (s, e) => Close();

            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }
}