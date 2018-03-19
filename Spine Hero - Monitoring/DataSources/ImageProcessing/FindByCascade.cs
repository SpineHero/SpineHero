using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpineHero.Monitoring.DataSources.ImageProcessing
{
    internal class FindByCascade
    {
        private CascadeClassifier cascade;
        private static Mutex mut = new Mutex();

        public FindByCascade(String faceFileName)
        {
            cascade = new CascadeClassifier(faceFileName);
        }

        public Rect? Detect(Mat image)
        {
            if (image == null) return null;
            Mat grayImage = new Mat();

            image.CopyTo(grayImage);
            if (image.Type().Channels == 3)
                grayImage.CvtColor(ColorConversionCodes.BGR2GRAY);

            mut.WaitOne();

            Rect[] detectedObject = cascade.DetectMultiScale(
               image,
               1.1,
               10,
               HaarDetectionType.ScaleImage,
               Size.Zero);

            mut.ReleaseMutex();
            grayImage.Dispose();

            List<Rect> locations = new List<Rect>();
            locations.AddRange(detectedObject);

            Rect location = new Rect(0, 0, 0, 0);
            foreach (Rect obj in locations)
            {
                if ((location.Width * location.Height) < (obj.Width * obj.Height))
                    location = obj;
            }
            if (location.Width == 0)
            {
                return null;
            }
            else
            {
                return location;
            }
        }
    }
}