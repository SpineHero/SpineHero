using System;
using System.Linq;
using OpenCvSharp;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;

namespace SpineHero.Monitoring.Watchers.DepthWatcher
{
    public class DepthImagePostureAnalyzer
    {
        private static readonly ILog log = Log.GetLogger<DepthImagePostureAnalyzer>();

        public BodyPoints AnalyzeDepthImage(ImageWrapper images)
        {
            if (images.DepthHead == null) return null;
            Moments m = Cv2.Moments(images.MaxAreaContour, true);
            Point centroid = new Point(m.M10 / m.M00, m.M01 / m.M00);
            return GetPoints(images.DepthImage, images.DepthImageMask, centroid, images.DepthHead.Value);
        }

        public BodyPoints GetPoints(Mat depthImage, Mat depthMask, Point center, Rect faceRect)
        {
            var mask = new MatOfByte(depthMask);
            var depth = new MatOfByte(depthImage);
            try
            {
                var body = FindBody(center, mask, depth);
                var head = FindHead(body, faceRect, mask, depth);
                return new BodyPoints(body, head);
            }
            catch (Exception e)
            {
                log.Debug(e.Message);
                return null;
            }
        }

        public BodyPart FindBody(Point center, MatOfByte mask, MatOfByte depth)
        {
            var body = new BodyPart(new Size(3, 3));
            Point l, r;
            Point[] cp;
            NormalizePoint(center, mask, out cp, out l, out r, center.Y - 10, mask.Height - 10, 10);
            body.Width = (int)(Math.Abs(r.X - l.X) * 0.9);
            body.Height = mask.Height - 10 - cp[0].Y;
            FillPoints((r.X + l.X) / 2 - body.Width / 2, cp[0].Y, body.Width, body.Height, depth, body);
            return body;
        }

        public BodyPart FindHead(BodyPart body, Rect face, MatOfByte mask, MatOfByte depth)
        {
            var head = new BodyPart(new Size(3, 3))
            {
                Width = body.Width / 3,
                Height = body.Width / 2
            };
            var h = new Point((face.Right + face.Left) / 2, (face.Bottom + face.Top) / 2);
            FillPoints(h.X - head.Width / 2, h.Y - head.Height / 2, head.Width, head.Height, depth, head);
            return head;
        }

        public void FillPoints(int x, int y, int width, int height, MatOfByte depth, BodyPart bp)
        {
            var m = bp.Size.Height;
            var n = bp.Size.Width;
            var w3 = width / m;
            var w32 = w3 / 2;
            var h3 = height / n;
            var h32 = h3 / 2;
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    var p = new PointValue(new Point(x + i * w3 - w32, y + j * h3 - h32), depth, h3, w3);
                    bp.Points[n * (j - 1) + (i - 1)] = p;
                }
            }
        }

        public void NormalizePoint(Point center, MatOfByte mask, out Point[] cp, out Point l, out Point r, int start, int stop, int offset)
        {
            int sycnt = (stop - start) / offset;
            if (sycnt <= 0) sycnt = 1;
            var xr = new int[sycnt];
            var xl = new int[sycnt];
            cp = new Point[sycnt];
            for (int i = 0; i < sycnt; i++, start += offset)
            {
                var rr = CastRay(mask, new Point(center.X, start), 90, 20);
                xr[i] = rr.X;
                var ll = CastRay(mask, new Point(center.X, start), -90, 20);
                xl[i] = ll.X;
                cp[i] = new Point((rr.X + ll.X) / 2, start);
            }
            r = new Point(xr.Sum() / sycnt, center.Y);
            l = new Point(xl.Sum() / sycnt, center.Y);
        }

        public Point CastRay(MatOfByte m, Point start, int angle, int maxDiff)
        {
            var indexer = m.GetIndexer();
            Point2d startF = start;
            Point2d vec = new Point2d(Math.Sin(angle * Math.PI / 180.0), Math.Cos(angle * Math.PI / 180.0));
            byte c = indexer[start.Y, start.X];
            while (true)
            {
                Point2d next = startF + vec;
                if (next.X < 0 || next.X >= m.Width || next.Y < 0 || next.Y >= m.Height)
                    break;
                var cc = indexer[(int)next.Y, (int)next.X];
                if (Math.Abs(cc - c) > maxDiff || cc == 0)
                    break;
                startF = next;
            }
            return new Point(startF.X, startF.Y);
        }
    }
}