using OpenCvSharp;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;
using System;
using System.Text;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.HeadWatcher
{
    public class HeadWatcher : CalibrationDependentWatcher, IColorImageWatcher
    {
        private static readonly ILog log = Log.GetLogger<HeadWatcher>();
        private Rect? calibHead;

        public HeadWatcher()
        {
        }

        public HeadWatcher(ImageWrapper calibrationImages) : base(calibrationImages)
        {
        }

        [LogMethodCall]
        public override ResultMessage AnalyzeImages(ImageWrapper images)
        {
            Posture detectedPosture;
            if (!IsInputCorrect(images)) return null;
            if (calibHead == null) return null;

            if (images.Head == null || images.Head?.Width == 0 || images.Head?.Height == 0)
            {
                return new ResultMessage(GetType().Name, 0, 100, Posture.Unknown);
            }

            var location = images.Head.Value;

            int objC = calibHead.Value.Width + calibHead.Value.Height;
            int objA = location.Width + location.Height;

            int sideShift = calibHead.Value.X + calibHead.Value.Width / 2 - (location.X + location.Width / 2);
            int heightShift = calibHead.Value.Y + calibHead.Value.Height / 2 - (location.Y + location.Height / 2);

            int pObj;
            double cons = 2;

            if (objC < objA)
            {
                pObj = (int)(objC / (float)objA * 100);
                pObj -= (int)((100 - pObj) * cons);
                detectedPosture = Posture.LeanForward;
            }
            else
            {
                pObj = (int)(objA / (float)objC * 100);
                pObj -= (int)((100 - pObj) * cons);
                detectedPosture = Posture.LeanBackward;
            }
            heightShift = Math.Abs(heightShift);
            pObj -= heightShift * 100 / calibHead.Value.Height;
            int absSideShift = Math.Abs(sideShift);

            if (pObj > 70)
            {
                detectedPosture = Posture.Correct;
            }

            if ((absSideShift * 100 / calibHead.Value.Width) > 30)
            {
                pObj = 100 - absSideShift * 100 / calibHead.Value.Width;
                detectedPosture = sideShift < 0 ? Posture.LeanLeft : Posture.LeanRight;
            }

            if (VisualizationEnabled)
                Visualization = Visualize(images.ColorImage, calibHead.Value, location, detectedPosture);

            pObj = pObj < 0 ? 0 : pObj;

            return new ResultMessage(GetType().Name, pObj, 100, detectedPosture);
        }

        public override void CalibrationImagesChanged(ImageWrapper images)
        {
            if (!IsInputCorrect(images)) return;
            base.CalibrationImagesChanged(images);
            calibHead = images.Head;
        }

        private bool IsInputCorrect(ImageWrapper images)
        {
            var errText = new StringBuilder();
            if (images == null)
                errText.Append("Images are null. ");
            else
            {
                if (images.ColorImage == null)
                    errText.Append("Color image is null. ");
            }
            if (errText.Length > 0)
            {
                log.Warn(errText.ToString());
                return false;
            }
            return true;
        }

        public Mat Visualize(Mat image, Rect calib, Rect current, Posture posture)
        {
            var mat = image.Clone();
            mat.Rectangle(calib, Scalar.Blue);
            mat.Rectangle(current, posture == Posture.Correct ? Scalar.Green : Scalar.Red);
            var flipped = mat.Flip(FlipMode.Y);
            mat.Dispose();
            return flipped;
        }
    }
}