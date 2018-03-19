using System.IO;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources.ImageProcessing;

namespace SpineHero.Monitoring.DataSources
{
    public class IntelF200DataSource : DataSource
    {
        private PXCMSession session;
        private PXCMSenseManager sm;
        private PXCMProjection projection;
        private bool disposed;

        public override bool LoadNext()
        {
            lock (locker)
            {
                if (!Running || sm == null) return false;
                if (sm.AcquireFrame(true).IsError()) return false;

                PXCMCapture.Sample sample = sm.QuerySample();
                PXCMImage colorImage = sample.color;
                PXCMImage depthImage = sample.depth;
                Mat colorMatImage = ProcessColorImage(colorImage);
                Mat depth = ProcessDepthImage(depthImage);
                Mat depthMatImage = depth.Resize(new Size(), 0.5, 0.5);
                Rect? cFace = GetFacePosition(sm.QueryFace()?.CreateOutput());
                Rect? dFace = GetFacePosition(cFace, depthImage);

                colorImage.Dispose();
                depthImage.Dispose();
                sm.ReleaseFrame();
                Images = new ImageWrapper(colorMatImage, depthMatImage, cFace, dFace);

                return true;
            }
        }

        private Rect? GetFacePosition(Rect? cFace, PXCMImage depthImage)
        {
            if (cFace == null) return null;
            var face = cFace.Value;
            var input = new[]
            {
                new PXCMPointF32(face.X + face.Width/2, face.Y + face.Height/2),
            };
            var uv = new PXCMPointF32[input.Length];
            var width = face.Width / 2;
            var height = face.Height / 2;
            projection.MapColorToDepth(depthImage, input, uv);
            return new Rect((int)(uv[0].x - width) / 2, (int)(uv[0].y - height) / 2, width, height);
        }

        private static Rect? GetFacePosition(PXCMFaceData faceData)
        {
            if (faceData == null) return null;
            faceData.Update();
            PXCMFaceData.Face face = faceData.QueryFaceByIndex(0);
            if (face == null) return null;

            PXCMFaceData.DetectionData ddata = face.QueryDetection();
            PXCMRectI32 rect;
            ddata.QueryBoundingRect(out rect);

            return new Rect(rect.x, rect.y, rect.w, rect.h);
        }

        public static Mat ProcessColorImage(PXCMImage image)
        {
            int width = image.info.width;
            int height = image.info.height;

            PXCMImage.ImageData data;

            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out data).IsSuccessful())
            {
                var cpixels = data.ToByteArray(0, width * height * 3);
                image.ReleaseAccess(data);
                return new Mat(height, width, MatType.CV_8UC3, cpixels);
            }
            return null;
        }

        public static Mat ProcessDepthImage(PXCMImage image)
        {
            int width = image.info.width;
            int height = image.info.height;

            PXCMImage.ImageData data;
            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, out data).IsSuccessful())
            {
                var dpixels = data.ToShortArray(0, width * height);
                var dp = new byte[dpixels.Length];
                for (int i = 0; i < dpixels.Length; i++)
                {
                    dp[i] = ImageUtils.TrimDepth(dpixels[i]);
                }
                image.ReleaseAccess(data);
                return new Mat(height, width, MatType.CV_8UC1, dp);
            }
            return null;
        }

        public override void Start()
        {
            lock (locker)
            {
                if (Running) return;
                session = PXCMSession.CreateInstance();
                if (session != null)
                {
                    sm = session.CreateSenseManager();
                    sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480);
                    sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480);
                    sm.EnableFace();
                    var config = sm.QueryFace().CreateActiveConfiguration();
                    config.detection.maxTrackedFaces = 1;
                    config.pose.isEnabled = false;
                    config.SetTrackingMode(PXCMFaceConfiguration.TrackingModeType.FACE_MODE_COLOR_PLUS_DEPTH);
                    config.strategy = PXCMFaceConfiguration.TrackingStrategyType.STRATEGY_CLOSEST_TO_FARTHEST;
                    config.landmarks.isEnabled = false;
                    config.ApplyChanges();
                    var init = sm.Init();
                    if (init.IsSuccessful())
                    {
                        Running = true;
                        projection = sm.captureManager.device.CreateProjection();

                    }
                    else
                    {
                        throw new IOException($"Intel sensor init failed ({init}).");
                    }
                }
                else
                {
                    throw new IOException("Intel sensor is not connected.");
                }
            }
        }

        public override void Stop()
        {
            lock (locker)
            {
                if (!Running) return;
                session.Dispose();
                sm.Dispose();
                projection.Dispose();
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
            base.Dispose(disposing);
        }
    }
}