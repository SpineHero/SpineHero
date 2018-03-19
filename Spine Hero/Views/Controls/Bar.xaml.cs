using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace SpineHero.Views.Controls
{
    /// <summary>
    /// Interaction logic for Bar.xaml
    /// </summary>
    public partial class Bar : UserControl
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.RegisterAttached(
            nameof(Items), typeof(ObservableCollection<BarItem>), typeof(Bar), new FrameworkPropertyMetadata(OnItemsChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(Bar), new PropertyMetadata(0));

        public Bar()
        {
            InitializeComponent();
        }

        public ObservableCollection<BarItem> Items
        {
            get { return (ObservableCollection<BarItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = d as Bar;
            if (bar == null) return;

            var old = (ObservableCollection<BarItem>)e.OldValue;
            if (old != null)
            {
                old.CollectionChanged -= bar.BarItemsCollectionChanged;
            }
            ((ObservableCollection<BarItem>)e.NewValue).CollectionChanged += bar.BarItemsCollectionChanged;
        }

        public void BarItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            foreach (var item in e.NewItems)
            {
                var barItem = item as BarItem;
                if (barItem == null) continue;
                barItem.Height = Height * (barItem.Value / (double)Value);
            }
        }
    }
}