using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SpineHero.Properties;
using System;
using System.Linq;

namespace SpineHero.Model.Graphs
{
    public class RecentSittingQuality
    {
        private static readonly int MAX_DISPLAY_TIME = Const.Default.RSQMaxDisplayTime;
        private static readonly int MAX_DISPLAY_TIME_GRAPH = Const.Default.RSQMaxDisplayTimeGraph;
        private static readonly int MAX_SEC_TO_CONECT_LINE = Const.Default.RSQMaxSecToConnectLine;
        private static readonly int GRAPH_PADDING = Const.Default.RSQGraphAbsoluteMaximumPadding;
        private static readonly int GRAPH_CLEAR_INTERAVAL = Const.Default.RSQGraphClearInterval;
        private DateTime lastTimeCleared = DateTime.Now;

        public RecentSittingQuality()
        {
            SetUpPlotController();
            SetUpPlotModel();
        }

        public PlotController PlotController { get; } = new PlotController();

        public PlotModel PlotModel { get; } = new PlotModel();

        private void SetUpPlotController()
        {
            PlotController.BindMouseEnter(PlotCommands.HoverTrack); //can be set only for data points
            PlotController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            PlotController.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, 2, PlotCommands.ResetAt);
            PlotController.UnbindMouseDown(OxyMouseButton.Right);
            PlotController.UnbindMouseWheel();
        }

        private void SetUpPlotModel() // TODO Use xaml?
        {
            PlotModel.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalType = DateTimeIntervalType.Minutes,
                MinorIntervalType = DateTimeIntervalType.Minutes,
                MinimumPadding = 0.05,
                MaximumPadding = 0.05
            };
            PlotModel.Axes.Add(dateAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MinorGridlineStyle = LineStyle.None,
                MajorGridlineStyle = LineStyle.Dash,
                TickStyle = TickStyle.Inside,
                MajorStep = 25,
                MinorStep = 25,
                Minimum = 0,
                Maximum = 101,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 100,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            };
            PlotModel.Axes.Add(valueAxis);
        }

        public void AddPoint(DateTime time, int value)
        {
            var lineSeries = (LineSeries)PlotModel.Series.LastOrDefault();
            if (lineSeries == null || lineSeries.Points.LastOrDefault().X < DateTimeAxis.ToDouble(time.AddSeconds(-MAX_SEC_TO_CONECT_LINE)))
            {
                lineSeries = new LineSeries { Color = OxyColor.FromRgb(0, 164, 162) };
                PlotModel.Series.Add(lineSeries);
            }
            lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), value));
        }

        public void UpdateGraph(DateTime time)
        {
            DateTimeAxis axis = (DateTimeAxis)PlotModel.Axes[0];
            axis.AbsoluteMaximum = axis.Maximum = DateTimeAxis.ToDouble(time.AddSeconds(GRAPH_PADDING));
            axis.Minimum = DateTimeAxis.ToDouble(time.AddSeconds(-MAX_DISPLAY_TIME_GRAPH));
            var maxTime = time.AddSeconds(-MAX_DISPLAY_TIME);
            axis.AbsoluteMinimum = DateTimeAxis.ToDouble(maxTime);
            if (lastTimeCleared < time.AddSeconds(-GRAPH_CLEAR_INTERAVAL)) RemoveOldLineSeries(maxTime);
        }

        public void RemoveOldLineSeries(DateTime time)
        {
            var series = PlotModel.Series;
            var dd = DateTimeAxis.ToDouble(time);
            foreach (var s in series.Cast<LineSeries>().ToArray())
            {
                s.Points.RemoveAll(p => p.X < dd);
                if (s.Points.Count == 0) series.Remove(s);
            }
            lastTimeCleared = DateTime.Now;
        }
    }
}