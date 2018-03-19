using NUnit.Framework;
using SpineHero.Common.Resources;
using SpineHero.Monitoring.DataSources;

namespace SpineHero.UnitTests.Data_Sources
{
    [TestFixture]
    internal class ImageWrapperTests : AssertionHelper
    {
        private ImageWrapper image;

        [OneTimeSetUp]
        public void Init()
        {
            image = ImageWrapper.Load(ResourceHelper.GetLocalResourcePath(@"\Resources\Correct.dat"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            image.Dispose();
        }

        [Test]
        public void CloneTest()
        {
            var clone = image.Clone();
            /* Color mat */
            var ptr = clone.ColorImage.CvPtr == image.ColorImage.CvPtr;
            var data = clone.ColorImage.Data == image.ColorImage.Data;
            Expect(ptr, False);
            Expect(data, False);
            /* Depth mat */
            ptr = clone.DepthImage.CvPtr == image.DepthImage.CvPtr;
            data = clone.DepthImage.Data == image.DepthImage.Data;
            Expect(ptr, False);
            Expect(data, False);
            /* Head */
            Expect(clone.Head == image.Head, True);
        }
    }
}