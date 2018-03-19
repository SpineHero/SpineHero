using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpineHero.Views.Controls
{
    public partial class Arc : UserControl
    {
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(int), typeof(Arc),
                new PropertyMetadata(5, OnPropertyChanged));

        public static readonly DependencyProperty StrokeColorProperty =
            DependencyProperty.Register(nameof(StrokeColor), typeof(Brush), typeof(Arc),
                new PropertyMetadata(new SolidColorBrush(Colors.Green), OnPropertyChanged));

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(Arc),
                new PropertyMetadata(0d, OnPropertyChanged));

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(Arc),
                new PropertyMetadata(234d, OnPropertyChanged));

        public static readonly DependencyProperty EndPercentageProperty =
            DependencyProperty.Register(nameof(EndPercentage), typeof(double), typeof(Arc),
                new PropertyMetadata(65d, OnPercentageChanged));

        public static readonly DependencyProperty StartPercentageProperty =
            DependencyProperty.Register(nameof(StartPercentage), typeof(double), typeof(Arc),
                new PropertyMetadata(0d, OnPercentageChanged));

        public Arc()
        {
            InitializeComponent();
        }

        public Brush StrokeColor
        {
            get { return (Brush)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public double StartPercentage
        {
            get { return (double)GetValue(StartPercentageProperty); }
            set { SetValue(StartPercentageProperty, value); }
        }

        public double EndPercentage
        {
            get { return (double)GetValue(EndPercentageProperty); }
            set { SetValue(EndPercentageProperty, value); }
        }

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue) return;
            Arc arc = sender as Arc;
            if (args.Property.Name == nameof(StartPercentage)) arc.StartAngle = arc.StartPercentage * 360 / 100;
            else if (args.Property.Name == nameof(EndPercentage)) arc.EndAngle = arc.EndPercentage * 360 / 100;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue) return;
            (sender as Arc).Render();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Render();
        }

        public void Render()
        {
            var size = Math.Min(ActualHeight, ActualWidth);
            var radius = size / 2 - StrokeThickness;
            var diff = EndAngle - StartAngle;

            if (!IsLoaded || radius <= 0) return;
            Point startPoint = ComputeCartesianCoordinate(StartAngle, radius);
            Point endPoint = diff >= 360 ? ComputeCartesianCoordinate(359.999, radius) : ComputeCartesianCoordinate(EndAngle, radius);

            PathRoot.Width = radius * 2 + StrokeThickness;
            PathRoot.Height = radius * 2 + StrokeThickness;
            PathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);
            PathFigure.StartPoint = startPoint;
            ArcSegment.Point = endPoint;
            ArcSegment.Size = new Size(radius, radius);
            var mod = ((diff % 360) + 360) % 360;
            ArcSegment.IsLargeArc = mod > 180 || diff >= 360d;
        }

        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x + radius, y + radius);
        }
    }
}