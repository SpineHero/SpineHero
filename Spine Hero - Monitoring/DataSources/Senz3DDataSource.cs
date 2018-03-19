extern alias OldSense;
using System;
using System.IO;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources.ImageProcessing;

namespace SpineHero.Monitoring.DataSources
{
    public class Senz3DDataSource : DataSource
    {
        private OldSense::UtilMPipeline pipeline;
        private OldSense::PXCMProjection projection;
        private bool disposed;

        public override bool LoadNext()
        {
            lock (locker)
            {
                if (!Running || pipeline.IsDisconnected()) return false;
                pipeline.AcquireFrame(true);
                OldSense::PXCMImage color, depth;
                OldSense::PXCMFaceAnalysis face;
                try
                {
                    color = pipeline.QueryImage(OldSense::PXCMImage.ImageType.IMAGE_TYPE_COLOR);
                    depth = pipeline.QueryImage(OldSense::PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
                    face = pipeline.QueryFace();
                }
                catch (Exception) { return false; }

                Mat colorImage = ProcessColorImage(color);
                Mat depthImage = ProcessDepthImage(depth);
                Rect? cFace = GetFacePosition(face);
                Rect? dFace = GetFacePosition(cFace, depth);

                Images = new ImageWrapper(colorImage, depthImage, cFace, dFace);
                color.Dispose();
                depth.Dispose();
                face.Dispose();
                pipeline.ReleaseFrame();
                return true;
            }
        }

        private Rect? GetFacePosition(Rect? cFace, OldSense::PXCMImage depthImage)
        {
            if (cFace == null) return null;
            var face = cFace.Value;
            OldSense::PXCMPointF32[] input = {
                new OldSense::PXCMPointF32()
            };
            input[0].x = face.X + face.Width / 2;
            input[0].y = face.Y + face.Height / 2;
            var width = face.Width / 2;
            var height = face.Height / 2;
            var size = new OldSense::PXCMSizeU32 { height = 1, width = 1 };
            projection.MapColorCoordinatesToDepth(depthImage, input, 0, size);
            return new Rect((int)(input[0].x - width*1.5) / 2, (int)(input[0].y - height*0.9) / 2, width, height);
        }

        private static Rect? GetFacePosition(OldSense::PXCMFaceAnalysis ft)
        {
            int fid; ulong ts;
            if (ft.QueryFace(0, out fid, out ts) < OldSense::pxcmStatus.PXCM_STATUS_NO_ERROR) return null;

            var fad = ft.DynamicCast<OldSense::PXCMFaceAnalysis.Detection>(OldSense::PXCMFaceAnalysis.Detection.CUID);
            OldSense::PXCMFaceAnalysis.Detection.Data ddata;
            if (fad.QueryData(fid, out ddata) < OldSense::pxcmStatus.PXCM_STATUS_NO_ERROR) return null;

            var rect = ddata.rectangle;
            return new Rect((int)rect.x, (int)rect.y, (int)rect.w, (int)rect.h);
        }

        private static Mat ProcessColorImage(OldSense::PXCMImage image)
        {
            int width = (int)image.info.width;
            int height = (int)image.info.height;

            OldSense::PXCMImage.ImageData data;

            var status = image.AcquireAccess(OldSense::PXCMImage.Access.ACCESS_READ, OldSense::PXCMImage.ColorFormat.COLOR_FORMAT_RGB24, out data);
            if (status < OldSense::pxcmStatus.PXCM_STATUS_NO_ERROR) return null;

            var cpixels = data.ToByteArray(0, width * height * 3);
            image.ReleaseAccess(ref data);
            return new Mat(height, width, MatType.CV_8UC3, cpixels);
        }

        private static Mat ProcessDepthImage(OldSense::PXCMImage image)
        {
            int width = (int)image.info.width;
            int height = (int)image.info.height;

            OldSense::PXCMImage.ImageData data;
            if (image.AcquireAccess(OldSense::PXCMImage.Access.ACCESS_READ, OldSense::PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, out data) >= OldSense::pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                var dpixels = data.ToShortArray(0, width * height);
                var dp = new byte[dpixels.Length];
                for (int i = 0; i < dpixels.Length; i++)
                {
                    dp[i] = ImageUtils.TrimDepth(dpixels[i]);
                }
                image.ReleaseAccess(ref data);
                return new Mat(height, width, MatType.CV_8UC1, dp);
            }
            return null; //TODO
        }

        public override void Start()
        {
            lock (locker)
            {
                if (Running) return;
                pipeline = new OldSense::UtilMPipeline();
                if (pipeline.capture == null) throw new IOException("Senz3D camera is not connected.");
                pipeline.EnableImage(OldSense::PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, 640, 480);
                pipeline.EnableImage(OldSense::PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH, 320, 240); // Doesn't work with 640x480
                pipeline.EnableFaceLocation();
                if (!pipeline.Init()) throw new IOException("Senz3D camera initialization process failed.");
                var dev = pipeline.capture.device;
                if (dev == null) throw new IOException("Senz3D camera is not connected.");
                int pid;
                dev.QueryPropertyAsUID(OldSense::PXCMCapture.Device.Property.PROPERTY_PROJECTION_SERIALIZABLE, out pid);
                pipeline.session.DynamicCast<OldSense::PXCMMetadata>(OldSense::PXCMMetadata.CUID).CreateSerializable(pid, OldSense::PXCMProjection.CUID, out projection);
                Running = true;
            }
        }

        public override void Stop()
        {
            lock (locker)
            {
                if (!Running) return;
                pipeline.Dispose();
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
        }
    }
}