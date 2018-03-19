using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.DepthWatcher
{
    public class DepthWatcher : CalibrationDependentWatcher, IDepthImageWatcher
    {
        private static readonly ILog log = Log.GetLogger<DepthWatcher>();
        private readonly DepthImagePostureAnalyzer analyzer = new DepthImagePostureAnalyzer();
        private readonly DepthImagePostureEvaluator evaluator = new DepthImagePostureEvaluator();
        private BodyPoints calibration;

        public DepthWatcher()
        {
        }

        public DepthWatcher(ImageWrapper calibrationImages) : base(calibrationImages)
        {
        }

        [LogMethodCall]
        public override ResultMessage AnalyzeImages(ImageWrapper images)
        {
            if (calibration == null) throw new ArgumentException("Calibration is not set!");
            if (!IsInputCorrect(images)) return null;
            BodyPoints points = analyzer.AnalyzeDepthImage(images);
            PostureEvaluation posture = evaluator.EvaluatePoints(points, calibration);

            if (VisualizationEnabled)
                Visualization = Visualize(images.DepthImageMask, points, posture);

            return new ResultMessage(GetType().Name, posture.SittingQuality, 100, posture.DetectedPosture);
        }

        public override void CalibrationImagesChanged(ImageWrapper images)
        {
            if (!IsInputCorrect(images)) return;
            base.CalibrationImagesChanged(images);
            calibration = analyzer.AnalyzeDepthImage(images);
        }

        public bool IsInputCorrect(ImageWrapper images)
        {
            var errText = new StringBuilder();
            if (images == null)
                errText.Append("Images are null. ");
            else
            {
                if (images.DepthImage == null || images.DepthImage.Empty())
                    errText.Append("Depth image is null. ");
                if (images.DepthImageMask == null || images.DepthImageMask.Empty())
                    errText.Append("Depth image mask is null. ");
                if (images.MaxAreaContour == null || images.MaxAreaContour.Length == 0)
                    errText.Append("Max area contour is null");
            }
            if (errText.Length > 0)
            {
                log.Warn(errText.ToString());
                return false;
            }
            return true;
        }

        public Mat Visualize(Mat mask, BodyPoints points, PostureEvaluation posture)
        {
            if (points == null || posture.DetectedPosture == Posture.Unknown) return null;
            var color = mask.CvtColor(ColorConversionCodes.GRAY2BGR);
            var mat = color.Flip(FlipMode.Y);
            color.Dispose();

            DrawPoints(mat, points.Head.Points, Scalar.Green);
            DrawPoints(mat, points.Body.Points, Scalar.Cyan);

            var p = new Point(mat.Width - points.Head[4].Point.X + 10, points.Head[4].Point.Y - 10);
            mat.PutText((points.Head[4].Value).ToString(), p, HersheyFonts.HersheyPlain, 1, Scalar.Red);
            var pp = new Point(mat.Width - points.Body[1].Point.X + 10, points.Body[1].Point.Y - 10);
            mat.PutText((points.Body[1].Value).ToString(), pp, HersheyFonts.HersheyPlain, 1, Scalar.Red);

            var x = mask.Width * 0.8;
            for (int i = 0; i < posture.Values.Length; i++)
            {
                mat.PutText(PostureEvaluation.Names[i] + $":{posture.Values[i],5:0.0}", new Point(x, 10 * i + 20), HersheyFonts.HersheyPlain, 0.7, posture.IsWrong(i) ? Scalar.Red : Scalar.Green);
            }

            return mat;
        }

        private static void DrawPoints(Mat image, IEnumerable<PointValue> points, Scalar color)
        {
            if (points == null) return;
            foreach (var p in points)
            {
                image.DrawMarker(image.Width - p.Point.X, p.Point.Y, color);
            }
        }
    }
}