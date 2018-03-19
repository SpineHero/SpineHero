using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpineHero.ViewModels.DialogWindows
{
    internal class VisualizationImagesViewModel : Screen, IHandle<IEnumerable<Mat>>
    {
        private ObservableCollection<Image> images;

        public VisualizationImagesViewModel(IEventAggregator events)
        {
            DisplayName = "Visualization";
            events.Subscribe(this);
            Images = new ObservableCollection<Image>();
        }

        public ObservableCollection<Image> Images { get { return images; } set { images = value; NotifyOfPropertyChange(() => Images); } }

        public void Handle(IEnumerable<Mat> message)
        {
            Images.Clear();
            foreach (var m in message.Where(x => x != null && !x.IsDisposed))
            {
                Images.Add(new Image
                {
                    Width = 480,
                    Height = 360,
                    Source = m.ToBitmapSource()
                });
            }
        }
    }
}