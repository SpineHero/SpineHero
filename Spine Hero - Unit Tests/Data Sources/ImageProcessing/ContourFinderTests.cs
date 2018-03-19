using NUnit.Framework;
using OpenCvSharp;
using SpineHero.Common.Resources;
using SpineHero.Monitoring.DataSources.ImageProcessing;
using System;

namespace SpineHero.UnitTests.Data_Sources.ImageProcessing
{
    [TestFixture]
    internal class ContourFinderTests : AssertionHelper
    {
        private Mat image;
        private Point[] maxContour;
        private const int area = 79 * 79;
        private const int cnt = 8;

        [OneTimeSetUp]
        public void Init()
        {
            var path = ResourceHelper.GetLocalResourcePath(@"Resources\Contour.png");
            image = new Mat(path);
            maxContour = ContourFinder.GetMaxAreaContour(image);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            image.Dispose();
        }

        [Test]
        public void GetContoursTest()
        {
            Point[][] points;
            HierarchyIndex[] hierarchy;
            ContourFinder.GetContours(image, out points, out hierarchy);
            Expect(points.GetLength(0), EqualTo(cnt));
        }

        [Test]
        public void GetMaxAreaContourWrongInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaxAreaContour(null));
            Assert.Throws<ArgumentException>(() => ContourFinder.GetMaxAreaContour(new Mat()));
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaxAreaContour(null, null));
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaxAreaContour(new Point[0][], new HierarchyIndex[0]));
        }

        [Test]
        public void GetMaxAreaContourTest()
        {
            Expect((int)Cv2.ContourArea(maxContour), EqualTo(area)); // Contour is 1 px smaller...
        }

        [Test]
        public void GetMaskFromContourWrongInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaskFromContour(null, null));
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaskFromContour(new Mat(), null));
            Assert.Throws<ArgumentNullException>(() => ContourFinder.GetMaskFromContour(null, maxContour));
        }

        [Test]
        public void GetMaskFromContourTest()
        {
            var mask = ContourFinder.GetMaskFromContour(image, maxContour);
            Expect(mask, !Null);
            var mac = ContourFinder.GetMaxAreaContour(mask);
            Expect((int)Cv2.ContourArea(mac), EqualTo(area));
        }
    }
}