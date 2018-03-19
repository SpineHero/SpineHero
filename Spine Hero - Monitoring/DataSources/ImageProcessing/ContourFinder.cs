using System;
using OpenCvSharp;
using PostSharp.Patterns.Contracts;

namespace SpineHero.Monitoring.DataSources.ImageProcessing
{
    public static class ContourFinder
    {
        public static Point[] GetMaxAreaContour([Required] Mat image)
        {
            if (image.Empty()) throw new ArgumentException("Image is empty.");
            var gray = image.Channels() == 1 ? image : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            GetContours(gray, out contours, out hierarchy);
            if (contours.Length == 0) return null;
            return GetMaxAreaContour(contours, hierarchy);
        }

        public static Point[] GetMaxAreaContour([NotEmpty] Point[][] contours, [NotEmpty] HierarchyIndex[] hierarchy)
        {
            var contourIndex = 0;
            var previousArea = 0;
            var maxContour = contours[0];

            while ((contourIndex >= 0))
            {
                var contour = contours[contourIndex];

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
                var boundingRectArea = boundingRect.Width * boundingRect.Height;
                if (boundingRectArea > previousArea)
                {
                    previousArea = boundingRectArea;
                    maxContour = contours[contourIndex];
                }
                contourIndex = hierarchy[contourIndex].Next;
            }
            return maxContour;
        }

        public static void GetContours([Required] Mat image, out Point[][] contours, out HierarchyIndex[] hierarchy)
        {
            var gray = (image.Channels() == 3) ? image.CvtColor(ColorConversionCodes.RGB2GRAY) : image.Clone();
            Cv2.FindContours(gray, out contours, out hierarchy, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            gray.Dispose();
        }

        public static Mat GetMaskFromContour([Required] Mat image, [NotEmpty] Point[] contour)
        {
            Mat mask = image.EmptyClone().SetTo(Scalar.Black);
            Point[][] mac = { contour };
            mask.DrawContours(mac, 0, Scalar.White, -1);
            return mask;
        }
    }
}