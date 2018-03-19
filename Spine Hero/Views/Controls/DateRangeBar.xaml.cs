using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpineHero.Model.Statistics;

namespace SpineHero.Views.Controls
{
    /// <summary>
    /// Interaction logic for DateRangeBar.xaml
    /// </summary>
    public partial class DateRangeBar : UserControl
    {
        private readonly SolidColorBrush SpineHeroColor = new SolidColorBrush(Color.FromRgb(0, 164, 162));

        public DateRange CurrentRange
        {
            get { return (DateRange)GetValue(CurrentRangeProperty); }
            set
            {
                UnselectButton();
                SelectButton(value);
                SetValue(CurrentRangeProperty, value);
            }
        }

        public static readonly DependencyProperty CurrentRangeProperty = DependencyProperty.Register(
            "CurrentRange", typeof(DateRange), typeof(DateRangeBar),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultValue = DateRange.Day,
            });

        public DateRangeBar()
        {
            InitializeComponent();
            CurrentRange = DateRange.Day;
        }

        private void UnselectButton()
        {
            SetButtonColors(CurrentRange, Brushes.Transparent, SpineHeroColor);
        }

        private void SelectButton(DateRange dateRange)
        {
            SetButtonColors(dateRange, SpineHeroColor, Brushes.White);
        }

        private void SetButtonColors(DateRange dateRange, SolidColorBrush background, SolidColorBrush foreground)
        {
            var button = ButtonForRange(dateRange);
            button.Background = background;
            button.Foreground = foreground;
        }

        private Button ButtonForRange(DateRange dateRange)
        {
            switch (dateRange)
            {
                case DateRange.Day:
                    return DayButton;
                case DateRange.Week:
                    return WeekButton;
                case DateRange.Month:
                    return MonthButton;
                case DateRange.Year:
                    return YearButton;
                default:
                    return DayButton;
            }
        }

        private void ChangeCurrentRange(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            CurrentRange = (DateRange) button.Tag;
        }
    }
}
