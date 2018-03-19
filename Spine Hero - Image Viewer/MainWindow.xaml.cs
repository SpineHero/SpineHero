using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.DepthWatcher;
using SpineHero.Monitoring.Watchers.HeadWatcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace SpineHero.ImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string EXT = Monitoring.Properties.Const.Default.IWFileExtension;
        private HeadWatcher headWatcher;
        private DepthWatcher depthWatcher;
        private ImageWrapper assigned;
        private ImageWrapper current;
        private IList<string> imagesPaths;
        private int index = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AssignImage(object sender, RoutedEventArgs e)
        {
            assigned = current.Clone();
            headWatcher = new HeadWatcher { VisualizationEnabled = true };
            headWatcher.CalibrationImagesChanged(assigned);
            depthWatcher = new DepthWatcher { VisualizationEnabled = true };
            depthWatcher.CalibrationImagesChanged(assigned);
            ShowImages();
        }

        public void AssignNull(object sender, RoutedEventArgs e)
        {
            assigned = null;
            ShowImages();
        }

        public void PreviousImage(object sender, RoutedEventArgs e)
        {
            if (imagesPaths == null)
                return;

            index--;
            if (index < 0) index = imagesPaths.Count - 1;
            ShowImages();
        }

        public void NextImage(object sender, RoutedEventArgs e)
        {
            if (imagesPaths == null)
                return;

            index++;
            if (index >= imagesPaths.Count) index = 0;
            ShowImages();
        }

        private void ShowImages()
        {
            if (imagesPaths == null || !imagesPaths.Any()) return;
            try
            {
                current = ImageWrapper.Load(imagesPaths[index]);
                ShowImages(current);
            }
            catch (Exception)
            {
                imagesPaths.RemoveAt(index);
                PreviousImage(null, null);
            }
        }

        public void ShowImages(ImageWrapper images)
        {
            if (images == null)
                return;
            if (assigned == null)
            {
                ColorImage.Source = images.ColorImage?.Flip(FlipMode.Y).ToBitmapSource();
                DepthImage.Source = images.DepthImage?.Flip(FlipMode.Y).ToBitmapSource();
            }
            else
            {
                try
                {
                    if (images.ColorImage == null) ColorImage.Source = null;
                    else if (assigned.ColorImage == null) ColorImage.Source = images.ColorImage?.Flip(FlipMode.Y).ToBitmapSource();
                    else
                    {
                        headWatcher.AnalyzeImages(images);
                        ColorImage.Source = headWatcher.Visualization?.ToBitmapSource();
                    }
                }
                catch (Exception) { ColorImage.Source = images.ColorImage?.Flip(FlipMode.Y).ToBitmapSource(); }
                try
                {
                    if (images.DepthImage == null) DepthImage.Source = null;
                    else if (assigned.DepthImage == null) DepthImage.Source = images.DepthImage?.Flip(FlipMode.Y).ToBitmapSource();
                    else
                    {
                        depthWatcher.AnalyzeImages(images);
                        DepthImage.Source = depthWatcher.Visualization?.ToBitmapSource();
                    }
                }
                catch (Exception) { DepthImage.Source = images.DepthImage?.Flip(FlipMode.Y).ToBitmapSource(); }
            }
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    PreviousImage(null, null); break;
                case Key.Right:
                    NextImage(null, null); break;
                case Key.Space:
                    AssignImage(null, null); break;
                case Key.Back:
                    AssignNull(null, null); break;
                case Key.L:
                    OpenFile(null, null); break;
                case Key.Escape:
                    Application.Current.Shutdown(); break;
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Filter = $"Image Wrapper Files(*{EXT}) | *{EXT}"
                };
                var showDialog = dialog.ShowDialog();
                if (showDialog != null && (bool)showDialog) SetImages(dialog.FileName);
            }
            catch (Exception ex)

            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetImages(string fileName)
        {
            imagesPaths = Directory.GetFiles(Path.GetDirectoryName(fileName)).Where(x => x.EndsWith(EXT)).ToList();
            index = imagesPaths.IndexOf(fileName);
            if (imagesPaths != null)
            {
                foreach (var b in BottomButtons.Children)
                {
                    (b as Button).IsEnabled = true;
                }
                ShowImages();
            }
            else
            {
                foreach (var b in BottomButtons.Children)
                {
                    (b as Button).IsEnabled = false;
                }
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                SetImages(args[1]);
            else
                OpenFile(null, null);
        }
    }
}