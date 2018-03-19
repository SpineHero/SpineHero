using OpenCvSharp;

namespace SpineHero.Monitoring.Watchers
{
    public interface ICanVisualize
    {
        Mat Visualization { get; }

        bool VisualizationEnabled { get; set; }
    }
}
