using Caliburn.Micro;
using SpineHero.Model.Statistics;
using SpineHero.Model.Store;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace SpineHero.ViewModels.MainMenuItems
{
    internal class HistoryStatisticsViewModel : Screen, IMainMenuItem
    {
        private readonly DatabaseQuery databaseQuery;
        private DateRange statisticsRange = DateRange.Day;

        public HistoryStatisticsViewModel(DatabaseQuery dq)
        {
            ResourceManager rm = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = rm.GetString("History");
            databaseQuery = dq;
        }

        public DateTime StatisticsDateTime { get; set; } = DateTime.Today;

        public DateRange StatisticsRange
        {
            get { return statisticsRange; }
            set
            {
                if (value == statisticsRange) return;
                statisticsRange = value;
                NotifyOfPropertyChange(() => StatisticsRange);
            }
        }

        public DateTime Date { get; set; }

        public DateRange Range { get; set; }

        public TimeSpan CorrectSittingTime { get; set; }

        public TimeSpan TotalSittingTime { get; set; }

        public TimeSpan LongestCorrectSittingTime { get; set; }

        public int Breaks { get; set; } = 3;

        public ObservableCollection<Bar> Bars { get; set; } = new ObservableCollection<Bar>();

        public List<HistoryData> HistoryData { get; set; }

        public void FillData(DateTime startTime, DateRange dateRange)
        {
            var endTime = dateRange.GetEndTime(startTime);
            HistoryData = databaseQuery.QueryHistoryData(startTime, dateRange);

            CorrectSittingTime = TimeSpan.FromMilliseconds(HistoryData.Sum(x => x.Correct));
            TotalSittingTime = TimeSpan.FromMilliseconds(HistoryData.Sum(x => x.Total));
            LongestCorrectSittingTime = databaseQuery.QueryLongestPostureTime(
                Posture.Correct, startTime, endTime);
            Breaks = databaseQuery.QueryCountPostureOccurrencesOfMinimalLength(
                Posture.Unknown, startTime, endTime, TimeSpan.FromMinutes(5));

            NotifyOfPropertyChange(() => CorrectSittingTime);
            NotifyOfPropertyChange(() => TotalSittingTime);
            NotifyOfPropertyChange(() => LongestCorrectSittingTime);
            NotifyOfPropertyChange(() => Breaks);

            //Bars.Clear();
            //for (int i = 0; i < 20; i++)
            //{
            //    var bar = new Bar();
            //    Bars.Add(bar);
            //    bar.Value = 100;
            //    bar.Width = 15;
            //    bar.Items = new ObservableCollection<BarItem>();
            //    bar.Items.Add(new BarItem { Value = i * 2, Height = bar.Height / 2, Background = Brushes.Green });
            //    bar.Items.Add(new BarItem { Value = i * 3, Background = Brushes.Red });
            //}
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            FillData(DateTime.Today, DateRange.Day);
        }
    }
}