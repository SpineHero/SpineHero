using System.Windows;
using System.Windows.Controls;

namespace SpineHero.Views
{
    public partial class ShellView
    {
        public ShellView()
        {
            MouseLeftButtonDown += delegate { DragMove(); };
            InitializeComponent();
#if !DEBUG
            ShowDebug.Visibility = Visibility.Hidden;
#endif
        }
    }
}