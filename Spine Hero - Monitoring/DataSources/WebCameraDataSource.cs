using DirectShowLib;
using OpenCvSharp;
using System.Collections.Generic;

namespace SpineHero.Monitoring.DataSources
{
    public class WebCameraDataSource : DataSource
    {
        private Capture capture;
        private bool disposed;
        private static readonly int cameraHeight = 480;
        private static readonly int cameraWidth = 640;

        public override bool LoadNext()
        {
            lock (locker)
            {
                if (!Running) return false;
                Mat colorImage = capture.Grab();
                if (colorImage == null) return false;
                Images = new ImageWrapper(colorImage);
                return true;
            }
        }

        public override void Start()
        {
            Start(0);
        }

        private void Start(int cameraNumber)
        {
            lock (locker)
            {
                if (Running) return;
                capture = new Capture(cameraNumber, 0, cameraWidth, cameraHeight);
                capture.Start();
                Running = true;
            }
        }

        public override void Stop()
        {
            lock (locker)
            {
                if (!Running) return;
                capture.Dispose();
                Running = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                }
                disposed = true;
            }
        }

        public static List<DsDevice> GetListOfCameras()
        {
            DsDevice[] cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            List<DsDevice> listCameras = new List<DsDevice>(cameras);
            return listCameras;
        }
    }
}