using Newtonsoft.Json;
using OpenCvSharp;
using PostSharp.Patterns.Contracts;
using SpineHero.Monitoring.DataSources.ImageProcessing;
using System;
using System.IO;

namespace SpineHero.Monitoring.DataSources
{
    public class ImageWrapper : IDisposable
    {
        private static readonly HeadFinder headFinder = new HeadFinder();
        private Mat depthImage;
        private Rect? head;
        private bool wasDetected;

        public ImageWrapper()
        {
        }

        public ImageWrapper(Mat color)
        {
            ColorImage = color;
        }

        public ImageWrapper(Mat color, Rect head) : this(color)
        {
            Head = head;
        }

        public ImageWrapper(Mat color, Mat depth) : this(color)
        {
            DepthImage = depth;
        }

        public ImageWrapper(Mat color, Mat depth, Rect? chead, Rect? dhead) : this(color, depth)
        {
            Head = chead;
            DepthHead = dhead;
        }

        ~ImageWrapper()
        {
            Dispose(false);
        }

        public Mat ColorImage { get; set; }

        public Mat DepthImage
        {
            get { return depthImage; }
            set
            {
                if (value == null)
                {
                    MaxAreaContour = null;
                    DepthImageMask = null;
                    depthImage = null;
                    return;
                }
                if (value.Empty()) throw new ArgumentException("Depth image is empty.");
                if (value.Type() != MatType.CV_8UC1) throw new ArgumentException($"Wrong mat type for depth image: {depthImage.Type()}.");
                depthImage = value;
                MaxAreaContour = ContourFinder.GetMaxAreaContour(depthImage);
                if (MaxAreaContour != null) DepthImageMask = ContourFinder.GetMaskFromContour(depthImage, MaxAreaContour);
            }
        }

        public Mat DepthImageMask { get; private set; }

        public Point[] MaxAreaContour { get; private set; }

        public Rect? Head
        {
            get
            {
                lock (this)
                {
                    if (head != null || wasDetected) return head;
                    head = headFinder.GetHead(ColorImage);
                    wasDetected = true;
                    return head;
                }
            }
            set
            {
                lock (this)
                {
                    head = value;
                    wasDetected = true;
                }
            }
        }

        public Rect? DepthHead { get; set; }

        public ImageWrapper Clone()
        {
            return new ImageWrapper
            {
                ColorImage = ColorImage?.Clone(),
                DepthImage = DepthImage?.Clone(),
                Head = Head,
                DepthHead = DepthHead
            };
        }

        public string ToJson()
        {
            var obj = new SerializedImages(this);
            return JsonConvert.SerializeObject(obj);
        }

        #region Save/Load Logic

        public void Save(string path)
        {
            Save(this, path);
        }

        public static void Save([NotNull(ErrorMessage = "Can't save empty images")] ImageWrapper images, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = File.Open(path, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    var obj = new SerializedImages(images);
                    string json = JsonConvert.SerializeObject(obj);
                    bw.Write(json);
                }
            }
        }

        public static ImageWrapper Load(string path)
        {
            using (var stream = File.Open(path, FileMode.Open))
            {
                SerializedImages obj;
                using (var br = new BinaryReader(stream))
                {
                    var json = br.ReadString();
                    obj = JsonConvert.DeserializeObject<SerializedImages>(json);
                }
                return obj.Deserialize();
            }
        }

        #endregion Save/Load Logic

        #region IDisposable Support

        private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                //Free managed objects here
            }
            ColorImage?.Dispose();
            DepthImage?.Dispose();
            DepthImageMask?.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }

    public class SerializedImages
    {
        public SerializedImages()
        {
        }

        public SerializedImages([NotNull] ImageWrapper images)
        {
            Color = images.ColorImage?.ToBytes();
            Depth = images.DepthImage?.ToBytes();
            Head = images.Head;
            DepthHead = images.DepthHead;
        }

        public byte[] Color { get; set; }
        public byte[] Depth { get; set; }
        public Rect? Head { get; set; }
        public Rect? DepthHead { get; set; }

        public ImageWrapper Deserialize()
        {
            var color = Color != null ? Mat.FromImageData(Color) : null;
            var depth = Depth != null ? Mat.FromImageData(Depth).CvtColor(ColorConversionCodes.BGR2GRAY) : null;
            var imgs = new ImageWrapper
            {
                ColorImage = color,
                DepthImage = depth,
                Head = Head,
                DepthHead = DepthHead
            };
            return imgs;
        }
    }
}