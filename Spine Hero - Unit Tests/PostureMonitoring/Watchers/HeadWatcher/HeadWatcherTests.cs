using NUnit.Framework;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Properties;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.PostureMonitoring.Watchers.HeadWatcher
{
    [TestFixture]
    internal class HeadWatcherTests : AssertionHelper
    {
        private Monitoring.Watchers.HeadWatcher.HeadWatcher headWatcher;
        private Mat color;
        private ImageWrapper calib;

        [SetUp]
        public void Init()
        {
            color = new Mat(480, 640, MatType.CV_8UC3);
            int size = 50;
            Rect head = new Rect(size, size, size, size);
            calib = new ImageWrapper(color, head);
        }

        [TearDown]
        public void CleanUp()
        {
            color.Dispose();
        }

        [Test]
        public void AnalyzeImagesWithoutCalibration()
        {
            headWatcher = new Monitoring.Watchers.HeadWatcher.HeadWatcher();
            ImageWrapper images = new ImageWrapper();
            Expect(null, EqualTo(headWatcher.AnalyzeImages(images)));
        }

        [Test]
        public void AnalyzeNullImages()
        {
            headWatcher = new Monitoring.Watchers.HeadWatcher.HeadWatcher(calib);
            Expect(null, EqualTo(headWatcher.AnalyzeImages(null)));
        }

        [Test]
        public void AnalyzeImageWithoutHead()
        {
            headWatcher = new Monitoring.Watchers.HeadWatcher.HeadWatcher(calib);
            ImageWrapper imageWraper = new ImageWrapper(color);
            Expect(Posture.Unknown, EqualTo(headWatcher.AnalyzeImages(imageWraper).DetectedPosture));
        }

        [Test]
        public void AnalyzeCorrectPostureImages()
        {
            headWatcher = new Monitoring.Watchers.HeadWatcher.HeadWatcher(calib);

            ResultMessage sittingEvaluation = headWatcher.AnalyzeImages(calib);
            Expect(100, EqualTo(sittingEvaluation.SittingQuality));
            Expect(Posture.Correct, EqualTo(sittingEvaluation.DetectedPosture));
        }

        [Test]
        public void AnalyzeWrongPostureImages()
        {
            headWatcher = new Monitoring.Watchers.HeadWatcher.HeadWatcher(calib);

            int size = 40;
            int startPosition = 45;
            Rect head = new Rect(startPosition, startPosition, size, size);
            ImageWrapper image = new ImageWrapper(color, head);

            ResultMessage sittingEvaluation = headWatcher.AnalyzeImages(image);
            Expect(Const.Default.CorrectLimit, GreaterThanOrEqualTo(sittingEvaluation.SittingQuality));
            Expect(Posture.LeanBackward, EqualTo(sittingEvaluation.DetectedPosture));

            size = 60;
            startPosition = 55;
            head = new Rect(startPosition, startPosition, size, size);
            image = new ImageWrapper(color, head);

            sittingEvaluation = headWatcher.AnalyzeImages(image);
            Expect(Const.Default.CorrectLimit, GreaterThanOrEqualTo(sittingEvaluation.SittingQuality));
            Expect(Posture.LeanForward, EqualTo(sittingEvaluation.DetectedPosture));

            size = 50;
            startPosition = 10;
            head = new Rect(startPosition, startPosition, size, size);
            image = new ImageWrapper(color, head);

            sittingEvaluation = headWatcher.AnalyzeImages(image);
            Expect(Const.Default.CorrectLimit, GreaterThanOrEqualTo(sittingEvaluation.SittingQuality));
            Expect(Posture.LeanRight, EqualTo(sittingEvaluation.DetectedPosture));

            startPosition = 100;
            head = new Rect(startPosition, startPosition, size, size);
            image = new ImageWrapper(color, head);

            sittingEvaluation = headWatcher.AnalyzeImages(image);
            Expect(Const.Default.CorrectLimit, GreaterThanOrEqualTo(sittingEvaluation.SittingQuality));
            Expect(Posture.LeanLeft, EqualTo(sittingEvaluation.DetectedPosture));
        }
    }
}