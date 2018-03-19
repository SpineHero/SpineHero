using NUnit.Framework;
using OpenCvSharp;
using SpineHero.Common.Resources;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.PostureMonitoring.Watchers.DepthWatcher
{
    [TestFixture]
    internal class DepthWatcherTests : AssertionHelper
    {
        private Monitoring.Watchers.DepthWatcher.DepthWatcher watcher;
        private ImageWrapper correct, leanForward, leanBackward;

        [OneTimeSetUp]
        public void Init()
        {
            correct = ImageWrapper.Load(ResourceHelper.GetLocalResourcePath(@"\Resources\Correct.dat"));
            leanBackward = ImageWrapper.Load(ResourceHelper.GetLocalResourcePath(@"\Resources\LeanBackward.dat"));
            leanForward = ImageWrapper.Load(ResourceHelper.GetLocalResourcePath(@"\Resources\LeanForward.dat"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            correct.Dispose();
            leanBackward.Dispose();
            leanForward.Dispose();
        }

        [SetUp]
        public void TestInit()
        {
            watcher = new Monitoring.Watchers.DepthWatcher.DepthWatcher();
        }

        [Test]
        public void IsInputCorrectTest()
        {
            Expect(watcher.IsInputCorrect(null), False);
            Expect(watcher.IsInputCorrect(new ImageWrapper()), False);
            Expect(watcher.IsInputCorrect(new ImageWrapper(correct.ColorImage)), False);
            Expect(watcher.IsInputCorrect(correct), True);
        }

        [Test]
        public void CalibrationImageChangedTest()
        {
            Expect(watcher.CalibrationIsSet, False);
            watcher.CalibrationImagesChanged(correct);
            Expect(watcher.CalibrationIsSet, True);
        }

        [Test]
        public void AnalyzeImagesTests()
        {
            watcher.CalibrationImagesChanged(correct);
            var img = new Mat(320, 240, MatType.CV_8UC1, Scalar.Black);
            var rm = watcher.AnalyzeImages(new ImageWrapper(img, img));
            img.Dispose();
            Expect(rm, Null);
            rm = watcher.AnalyzeImages(correct);
            Expect(rm.SittingQuality, EqualTo(100));
            Expect(rm.DetectedPosture, EqualTo(Posture.Correct));
            rm = watcher.AnalyzeImages(leanForward);
            Expect(rm.DetectedPosture, EqualTo(Posture.LeanForward));
            rm = watcher.AnalyzeImages(leanBackward);
            Expect(rm.DetectedPosture, EqualTo(Posture.LeanBackward));
        }
    }
}