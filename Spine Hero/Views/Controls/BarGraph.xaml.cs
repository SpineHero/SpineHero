using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpineHero.Views.Controls
{
    /// <summary>
    /// Interaction logic for BarGraph.xaml
    /// </summary>
    public partial class BarGraph : UserControl
    {
        public static readonly DependencyProperty AxesColorProperty = DependencyProperty.Register(
            nameof(AxesColor), typeof(Brush), typeof(BarGraph), new PropertyMetadata(Brushes.DimGray));

        public static readonly DependencyProperty AxesTextColorProperty = DependencyProperty.Register(
            nameof(AxesTextColor), typeof(Brush), typeof(BarGraph), new PropertyMetadata(Brushes.DimGray));

        public static readonly DependencyProperty BarsProperty = DependencyProperty.Register(
                    nameof(Bars), typeof(ObservableCollection<Bar>), typeof(BarGraph), new FrameworkPropertyMetadata(OnBarsChanged));

        public BarGraph()
        {
            InitializeComponent();
        }

        public Brush AxesColor
        {
            get { return (Brush)GetValue(AxesColorProperty); }
            set { SetValue(AxesColorProperty, value); }
        }

        public Brush AxesTextColor
        {
            get { return (Brush)GetValue(AxesTextColorProperty); }
            set { SetValue(AxesTextColorProperty, value); }
        }

        public ObservableCollection<Bar> Bars
        {
            get { return (ObservableCollection<Bar>)GetValue(BarsProperty); }
            set { SetValue(BarsProperty, value); }
        }

        private static void OnBarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = d as BarGraph;
            if (bar == null) return;

            var old = (ObservableCollection<Bar>)e.OldValue;
            if (old != null)
            {
                old.CollectionChanged -= bar.BarsCollectionChanged;
            }
            ((ObservableCollection<Bar>)e.NewValue).CollectionChanged += bar.BarsCollectionChanged;
        }

        public void BarsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            foreach (var bar in e.NewItems)
            {
                var barItem = bar as Bar;
                if (barItem == null) continue;
                barItem.Height = ItemsControl.ActualHeight;
            }
        }
    }
}