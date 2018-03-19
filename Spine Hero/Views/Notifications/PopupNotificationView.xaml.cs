using System.Windows;

namespace SpineHero.Views.Notifications
{
    /// <summary>
    /// Interaction logic for PopupNotificationView.xaml
    /// </summary>
    public partial class PopupNotificationView : Window
    {
        public PopupNotificationView()
        {
            InitializeComponent();
            MouseLeftButtonDown += delegate { this.DragMove(); }; // for moving with borderless window
        }
    }
}