using NUnit.Framework;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources.ImageProcessing;

namespace SpineHero.UnitTests.Data_Sources.ImageProcessing
{
    [TestFixture]
    public class ImageUtilsTest : AssertionHelper
    {
        private Mat image;

        [OneTimeSetUp]
        public void Init()
        {
            byte[,] array = new byte[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    array[i, j] = (byte)(i + j);
                }
            }
            image = new Mat(10, 10, MatType.CV_8UC1, array);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            image.Dispose();
        }

        [
            TestCase(500, 55),
            TestCase(501, 55),
            TestCase(502, 55),
            TestCase(503, 55),
            TestCase(504, 56),
            TestCase(0, 0),
            TestCase(10000, 0),
            TestCase(280, 0),
            TestCase(1300, 255)
        ]
        public void TestDepthTrim(short depth, byte correct)
        {
            byte result = ImageUtils.TrimDepth(depth);
            Expect(result, EqualTo(correct));
        }

        [Test]
        public void GetDepthValueTestBlackAndWhite()
        {
            var black = new Mat(20, 20, MatType.CV_8UC1, Scalar.Black);
            var white = new Mat(20, 20, MatType.CV_8UC1, Scalar.White);
            var v = ImageUtils.GetDepthValue(black, new Point(0, 0), 10, 10);
            Expect(v, EqualTo(0));
            v = ImageUtils.GetDepthValue(white, new Point(0, 0), 10, 10);
            Expect(v, EqualTo(255));
            black.Dispose();
            white.Dispose();
        }

        [TestCase(0, 0, 1, 1, 0)]
        [TestCase(9, 9, 1, 1, 18)]
        [TestCase(4, 1, 1, 5, (1 + 10) / 2)]
        [TestCase(5, 5, 5, 1, (5 + 15) / 2)]
        [TestCase(5, 5, 5, 5, 10)]
        public void GetDepthValueTests(int x, int y, int m, int n, int r)
        {
            var v = ImageUtils.GetDepthValue(image, new Point(x, y), m, n);
            Expect(v, EqualTo(r));
        }
    }
}