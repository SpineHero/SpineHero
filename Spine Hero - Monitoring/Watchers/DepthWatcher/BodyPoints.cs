using System.Linq;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources.ImageProcessing;

namespace SpineHero.Monitoring.Watchers.DepthWatcher
{
    public class BodyPoints
    {
        public BodyPart Head { get; }

        public BodyPart Body { get; }

        public BodyPoints(BodyPart body, BodyPart head)
        {
            Head = head;
            Body = body;
        }
    }

    public class BodyPart
    {
        private double avg = -1;
        private double[] avgr;
        private double[] avgc;

        public BodyPart(Size size)
        {
            Size = size;
            InitArrays();
        }

        private void InitArrays()
        {
            Points = new PointValue[Size.Height * Size.Width];
            avgr = new double[Size.Height];
            avgc = new double[Size.Width];
            for (int i = 0; i < avgr.Length; i++)
                avgr[i] = -1;
            for (int i = 0; i < avgc.Length; i++)
                avgc[i] = -1;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public Size Size { get; }

        public PointValue[] Points { get; private set; }

        public PointValue this[int i]
        {
            get { return Points[i]; }
            set { Points[i] = value; }
        }

        public PointValue this[int i, int j]
        {
            get { return Points[Size.Width * i + j]; }
            set { Points[Size.Width * i + j] = value; }
        }

        public double Average()
        {
            if (avg < 0)
                return avg = Points.Where(x => x.Value != 0).DefaultIfEmpty().Average(x => x.Value);
            return avg;
        }

        public double AverageColumn(int c)
        {
            if (avgc[c] >= 0) return avgc[c];
            int sum = 0, cnt = 0;
            for (int i = 0; i < Size.Height; i++)
            {
                var a = this[i, c].Value;
                if (a > 0)
                {
                    sum += a;
                    cnt++;
                }
            }
            return avgc[c] = cnt == 0 ? 0 : sum / cnt;
        }

        public double AverageRow(int r)
        {
            if (avgr[r] >= 0) return avgr[r];
            int sum = 0, cnt = 0;
            for (int i = 0; i < Size.Height; i++)
            {
                var a = this[r, i].Value;
                if (a > 0)
                {
                    sum += a;
                    cnt++;
                }
            }
            return avgr[r] = cnt == 0 ? 0 : sum / cnt;
        }
    }

    public struct PointValue
    {
        public Point Point { get; set; }

        public int Value { get; set; }

        public PointValue(Point point, Mat image) : this(point, image, 1, 1)
        {
        }

        public PointValue(Point point, Mat image, int m, int n)
        {
            Point = point;
            Value = ImageUtils.GetDepthValue(image, point, m, n);
        }
    }
}