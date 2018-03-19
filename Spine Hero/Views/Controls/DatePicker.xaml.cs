using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SpineHero.Model.Statistics;

namespace SpineHero.Views.Controls
{
    /// <summary>
    /// Interaction logic for DatePicker.xaml
    /// </summary>
    public partial class DatePicker : UserControl
    {
        public DateRange CurrentRange
        {
            get { return (DateRange)GetValue(CurrentRangeProperty); }
            set { SetValue(CurrentRangeProperty, value); }
        }

        public static readonly DependencyProperty CurrentRangeProperty = DependencyProperty.Register(
            "CurrentRange", typeof(DateRange), typeof(DatePicker),
            new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
                DefaultValue = DateRange.Day,
                PropertyChangedCallback = OnCurrentDateTimeOrRangeChanged
            });

        public DateTime CurrentDateTime
        {
            get { return (DateTime) GetValue(CurrentDateTimeProperty); }
            set { SetValue(CurrentDateTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentDateTimeProperty = DependencyProperty.Register(
            "CurrentDateTime", typeof (DateTime), typeof (DatePicker),
            new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
                DefaultValue = DateTime.Today,
                PropertyChangedCallback = OnCurrentDateTimeOrRangeChanged
            });

        public DatePicker()
        {
            InitializeComponent();
            UpdateText();
        }

        private static void OnCurrentDateTimeOrRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var datePicker = d as DatePicker;
            datePicker.UpdateText();
        }

        private void UpdateText()
        {
            DateTextBlock.Text = DateTimeToText(CurrentDateTime);
        }

        private string DateTimeToText(DateTime value)
        {
            switch (CurrentRange)
            {
                case DateRange.Day:
                    return value.ToString("d");
                case DateRange.Week:
                    var culture = CultureInfo.CurrentCulture;
                    var dateTimeInfo = DateTimeFormatInfo.GetInstance(culture);
                    int weekNumber = culture.Calendar.GetWeekOfYear(value, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);
                    return weekNumber + ". week";
                case DateRange.Month:
                    return value.ToString("Y");
                case DateRange.Year:
                    return value.ToString("yyyy");
            }
            return value.ToString("d"); ;
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            AddToCurrentDateTime(-1);
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            AddToCurrentDateTime(1);
        }

        private void AddToCurrentDateTime(int value)
        {
            switch (CurrentRange)
            {
                case DateRange.Day:
                    CurrentDateTime = CurrentDateTime.AddDays(value);
                    break;
                case DateRange.Week:
                    var weekValue = value*7;
                    CurrentDateTime = CurrentDateTime.AddDays(weekValue);
                    break;
                case DateRange.Month:
                    CurrentDateTime = CurrentDateTime.AddMonths(value);
                    break;
                case DateRange.Year:
                    CurrentDateTime = CurrentDateTime.AddYears(value);
                    break;
            }
        }
    }
}
