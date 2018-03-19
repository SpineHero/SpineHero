using OpenCvSharp;
using SpineHero.Common.Resources;

namespace SpineHero.Monitoring.DataSources.ImageProcessing
{
    internal class HeadFinder
    {
        public static readonly string FACE_CASCADE = ResourceHelper.GetLocalResourcePath(@"Resources\haarcascade_frontalface_default.xml");
        private readonly FindByCascade headCascade;

        public HeadFinder()
        {
            headCascade = new FindByCascade(FACE_CASCADE);
        }

        public Rect? GetHead(Mat image)
        {
            return headCascade.Detect(image);
        }
    }
}