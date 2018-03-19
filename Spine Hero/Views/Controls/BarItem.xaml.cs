using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpineHero.Views.Controls
{
    /// <summary>
    /// Interaction logic for BarItem.xaml
    /// </summary>
    public partial class BarItem : UserControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(BarItem), new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
            nameof(IconVisibility), typeof(Visibility), typeof(BarItem), new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(BarItem), new PropertyMetadata(0));

        public BarItem()
        {
            InitializeComponent();
        }
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Visibility IconVisibility
        {
            get
            {
                return (Visibility)GetValue(IconVisibilityProperty);
            }
            set
            {
                SetValue(IconVisibilityProperty, value);
            }
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}