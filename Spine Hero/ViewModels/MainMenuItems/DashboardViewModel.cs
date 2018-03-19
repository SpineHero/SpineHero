using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using Infralution.Localization.Wpf;
using SpineHero.Model.Graphs;
using SpineHero.Model.Statistics;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.Utils.Extentions;
using System;
using System.Reflection;
using System.Resources;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace SpineHero.ViewModels.MainMenuItems
{
    public sealed class DashboardViewModel : Screen, IMainMenuItem, IHandle<Evaluation>, IHandle<PostureMonitoringStatusChange>
    {
        private static readonly int GRAPTH_UPDATE_INTERVAL = Const.Default.RSQGraphUpdateInterval;
        private readonly StatisticsModule statisticsModule;
        private readonly BreaksModule breaksModule;
        private readonly DispatcherTimer timer = new DispatcherTimer();

        private string actualPostureImagePath;
        private double actualSittingQuality;

        public DashboardViewModel(IEventAggregator aggregator, StatisticsModule statisticsModule, BreaksModule breaksModule)
        {
            ResourceManager rm = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = rm.GetString("Dashboard");
            aggregator.Subscribe(this);
            this.statisticsModule = statisticsModule;
            this.breaksModule = breaksModule;
            SetUpTimer();
            ResetToDefault();
            CultureManager.UICultureChanged += (o, e) => { DisplayName = rm.GetString("Dashboard"); };
        }

        public string ActualPostureImagePath
        {
            get { return actualPostureImagePath; }
            set
            {
                actualPostureImagePath = value;
                NotifyOfPropertyChange(() => ActualPostureImagePath);
            }
        }

        public double ActualSittingQuality
        {
            get { return actualSittingQuality; }
            set
            {
                actualSittingQuality = value;
                NotifyOfPropertyChange(() => ActualSittingQuality);
            }
        }

        public TimeSpan SittingWithoutBreak => breaksModule.SittingStart == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now - breaksModule.SittingStart;

        public RecentSittingQuality RecentSittingQuality { get; private set; }

        public void ResetToDefault()
        {
            ActualPostureImagePath = Posture.Unknown.GetImageRepresentation();
            ActualSittingQuality = 0;
        }

        public void Handle(Evaluation eval)
        {
            if (breaksModule.SittingWithoutBreakForTooLong())
            {
                ((IShell)Parent).ShowNotification("Sitting without break", $"You are sitting for {(uint)SittingWithoutBreak.TotalMinutes} minutes. Take a break.", PopupAnimation.Fade, 10000);
            }

            if (!IsActive) return; // Dashboard is not active, there is no need to process evaluations.
            if (eval.Posture == Posture.Unknown)
            {
                ResetToDefault();
                return;
            }
            var sum = statisticsModule.LastSittingQualityAveraged;
            ActualSittingQuality = sum;
            ActualPostureImagePath = eval.Posture.GetImageRepresentation();
            RecentSittingQuality.AddPoint(eval.EvaluatedAt, sum);
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (!message.IsMonitoring)
            {
                ResetToDefault();
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            RecentSittingQuality = new RecentSittingQuality();
            var evalAdjusted = statisticsModule.SittingQualityAveraged;
            var evaluations = statisticsModule.Evaluations;
            for (int i = 0; i < evalAdjusted.Count; i++)
            {
                RecentSittingQuality.AddPoint(evaluations[i].EvaluatedAt, evalAdjusted[i]);
            }
            RecentSittingQuality.UpdateGraph(DateTime.Now);
            timer.Start();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            timer.Stop();
            RecentSittingQuality = null;
        }

        private void SetUpTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, GRAPTH_UPDATE_INTERVAL);
            timer.Tick += delegate
            {
                RecentSittingQuality.UpdateGraph(DateTime.Now);
                RecentSittingQuality.PlotModel.InvalidatePlot(true);
                NotifyOfPropertyChange(() => SittingWithoutBreak);
            };
        }
    }
}