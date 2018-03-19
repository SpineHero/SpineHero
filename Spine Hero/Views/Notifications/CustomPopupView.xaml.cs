using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SpineHero.Views.Notifications
{
    /// <summary>
    /// Interaction logic for GenericPopupView.xaml
    /// </summary>
    public partial class GenericPopupView : UserControl
    {
        public GenericPopupView()
        {
            InitializeComponent();
        }

        public GenericPopupView(string title, string text) : this()
        {
            Title.Text = title;
            Text.Text = text;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            (Parent as Popup).IsOpen = false;
        }
    }
}