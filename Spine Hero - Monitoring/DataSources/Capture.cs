using DirectShowLib;
using OpenCvSharp;
using SpineHero.Common.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SpineHero.Monitoring.DataSources
{
    internal class Capture : ISampleGrabberCB, IDisposable
    {
        #region Member variables

        private IFilterGraph2 filterGraph;
        private IMediaControl mediaControl;

        /// <summary> Wait for the async job to finish </summary>
        private readonly ManualResetEvent pictureReady;

        private volatile bool gotOne;
        private bool running;

        /// <summary> Image data </summary>
        private IntPtr ipBuffer = IntPtr.Zero;

        private static readonly ILog log = Log.GetLogger<Capture>();

        #endregion Member variables

        #region APIs

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr destination, IntPtr source,
            [MarshalAs(UnmanagedType.U4)] int length);

        #endregion APIs

        #region Ctor

        public Capture() : this(0, 0, 0, 0)
        {
        }

        public Capture(int deviceNum) : this(deviceNum, 0, 0, 0)
        {
        }

        public Capture(int deviceNum, int framerate) : this(deviceNum, framerate, 0, 0)
        {
        }

        public Capture(int deviceNum, int framerate, int width, int height)
        {
            var capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            if (deviceNum + 1 > capDevices.Length)
            {
                throw new ArgumentException($"No video capture devices found at index: {deviceNum}");
            }

            try
            {
                SetupGraph(capDevices[deviceNum], framerate, width, height);

                // Tell the callback to ignore new images
                pictureReady = new ManualResetEvent(false);
                gotOne = true;
                running = false;
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);
                Dispose();
                throw;
            }
        }

        ~Capture()
        {
            Dispose();
        }

        #endregion Ctor

        #region Properties

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Stride { get; private set; }

        #endregion Properties

        #region Setup

        private void SetupGraph(DsDevice device, int framerate, int width, int height)
        {
            ISampleGrabber sampleGrabber = null;
            IBaseFilter captureFilter = null;
            ICaptureGraphBuilder2 captureGraph = null;

            filterGraph = (IFilterGraph2)new FilterGraph();
            mediaControl = filterGraph as IMediaControl;

            try
            {
                // Get the ICaptureGraphBuilder2
                captureGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

                // Get the SampleGrabber interface
                sampleGrabber = (ISampleGrabber)new SampleGrabber();

                // Start building the graph
                var hr = captureGraph.SetFiltergraph(filterGraph);
                DsError.ThrowExceptionForHR(hr);

                // Add the video device
                hr = filterGraph.AddSourceFilterForMoniker(device.Mon, null, "Video input", out captureFilter);
                DsError.ThrowExceptionForHR(hr);

                var baseGrabFlt = (IBaseFilter)sampleGrabber;
                ConfigureSampleGrabber(sampleGrabber);

                // Add the frame grabber to the graph
                hr = filterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
                DsError.ThrowExceptionForHR(hr);

                // If any of the default config items are set
                if (framerate + height + width > 0)
                {
                    SetConfigParms(captureGraph, captureFilter, framerate, width, height);
                }

                hr = captureGraph.RenderStream(PinCategory.Capture, MediaType.Video, captureFilter, null, baseGrabFlt);
                DsError.ThrowExceptionForHR(hr);

                SaveSizeInfo(sampleGrabber);
            }
            finally
            {
                if (captureFilter != null)
                {
                    Marshal.ReleaseComObject(captureFilter);
                }
                if (sampleGrabber != null)
                {
                    Marshal.ReleaseComObject(sampleGrabber);
                }
                if (captureGraph != null)
                {
                    Marshal.ReleaseComObject(captureGraph);
                }
            }
        }

        private void ConfigureSampleGrabber(ISampleGrabber sampleGrabber)
        {
            // Set the media type to Video/RBG24
            var media = new AMMediaType
            {
                majorType = MediaType.Video,
                subType = MediaSubType.RGB24,
                formatType = FormatType.VideoInfo
            };
            var hr = sampleGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);

            // Configure the samplegrabber
            hr = sampleGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        private void SetConfigParms(ICaptureGraphBuilder2 captureGraph, IBaseFilter captureFilter, int framerate, int width, int height)
        {
            object o;
            AMMediaType media;

            // Find the stream config interface
            var hr = captureGraph.FindInterface(
                PinCategory.Capture, MediaType.Video, captureFilter, typeof(IAMStreamConfig).GUID, out o);
            DsError.ThrowExceptionForHR(hr);

            var videoStreamConfig = o as IAMStreamConfig;
            if (videoStreamConfig == null)
            {
                throw new Exception("Failed to get IAMStreamConfig");
            }

            // Get the existing format block
            hr = videoStreamConfig.GetFormat(out media);
            DsError.ThrowExceptionForHR(hr);

            // Copy out the videoinfoheader
            var videoInfoHeader = new VideoInfoHeader();
            Marshal.PtrToStructure(media.formatPtr, videoInfoHeader);

            if (framerate > 0)
            {
                videoInfoHeader.AvgTimePerFrame = 10000000 / framerate;
            }

            if (width > 0)
            {
                videoInfoHeader.BmiHeader.Width = width;
            }

            if (height > 0)
            {
                videoInfoHeader.BmiHeader.Height = height;
            }

            // Copy the media structure back
            Marshal.StructureToPtr(videoInfoHeader, media.formatPtr, false);

            // Set the new format
            hr = videoStreamConfig.SetFormat(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
        }

        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            var media = new AMMediaType();

            var hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            VideoInfoHeader videoInfoHeader =
                (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            Width = videoInfoHeader.BmiHeader.Width;
            Height = videoInfoHeader.BmiHeader.Height;
            Stride = Width * (videoInfoHeader.BmiHeader.BitCount / 8);

            DsUtils.FreeAMMediaType(media);
        }

        #endregion Setup

        #region Image Retrieval

        public Mat Grab()
        {
            if (!running) return null;

            ipBuffer = Marshal.AllocCoTaskMem(Stride * Height);

            try
            {
                pictureReady.Reset();
                gotOne = false;

                if (!pictureReady.WaitOne(5000, false))
                {
                    throw new Exception("Timeout waiting to get picture");
                }
                var mat = new Mat(Height, Width, MatType.CV_8UC3, ipBuffer);
                var flipped = mat.Flip(FlipMode.X);
                mat.Dispose();
                return flipped;
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);
                return null;
            }
            finally
            {
                Marshal.FreeCoTaskMem(ipBuffer);
                ipBuffer = IntPtr.Zero;
            }
        }

        public void Start()
        {
            if (!running)
            {
                int hr = mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);
                running = true;
            }
        }

        public void Pause()
        {
            if (running)
            {
                int hr = mediaControl.Pause();
                DsError.ThrowExceptionForHR(hr);
                running = false;
            }
        }

        public void Stop()
        {
            if (running)
            {
                int hr = mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
                running = false;
            }
        }

        private void CloseInterfaces()
        {
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);
            }

            if (filterGraph != null)
            {
                Marshal.ReleaseComObject(filterGraph);
                filterGraph = null;
            }
        }

        #endregion Image Retrieval

        #region Interface implementation

        /// <summary> Sample callback, NOT USED. </summary>
        int ISampleGrabberCB.SampleCB(double sampleTime, IMediaSample pSample)
        {
            if (!gotOne)
            {
                gotOne = true;

                IntPtr pBuffer;
                pSample.GetPointer(out pBuffer);

                if (pSample.GetSize() > Stride * Height)
                {
                    throw new Exception("Buffer is wrong size");
                }

                CopyMemory(ipBuffer, pBuffer, Stride * Height);

                pictureReady.Set();
            }

            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        int ISampleGrabberCB.BufferCB(double sampleTime, IntPtr pBuffer, int bufferLenght)
        {
            if (!gotOne)
            {
                if (bufferLenght <= Stride * Height)
                {
                    CopyMemory(ipBuffer, pBuffer, Stride * Height);
                }
                else
                {
                    throw new Exception("Buffer is wrong size");
                }
                gotOne = true;

                // Picture is ready.
                pictureReady.Set();
            }
            return 0;
        }

        public void Dispose()
        {
            CloseInterfaces();
            pictureReady?.Close();
        }

        #endregion Interface implementation
    }
}