using SpineHero.Monitoring.DataSources;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace SpineHero.ImageSampler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string EXT = Monitoring.Properties.Const.Default.IWFileExtension;
        private IDataSource dataSource;
        private string directory;
        private DispatcherTimer timer = new DispatcherTimer();
        

    public MainWindow()
        {
            InitializeComponent();
        }

        private void TakeImage(object sender, RoutedEventArgs e)
        {
            dataSource?.Images.Save(Path.Combine(directory, $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")},{dataSource?.GetType().Name},{(sender as Button)?.Content}{EXT}"));
        }

        private void ChangeDataSource(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox)?.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                if (item == Stop)
                {
                    timer?.Stop();
                    dataSource?.Stop();
                    return;
                }
                try
                {
                    IDataSource ds = null;
                    if (item == WebCamera)
                        ds = DataSourceFactory.CreateNewIfNeeded(dataSource, typeof(WebCameraDataSource));
                    else if (item == F200)
                        ds = DataSourceFactory.CreateNewIfNeeded(dataSource, typeof(IntelF200DataSource));
                    else if (item == Senz3D)
                        ds = DataSourceFactory.CreateNewIfNeeded(dataSource, typeof(Senz3DDataSource));
                    if (ds != dataSource) dataSource?.Stop();
                    dataSource = ds;
                    dataSource.Start();
                    timer.Interval = TimeSpan.FromMilliseconds(100);
                    timer.Tick += delegate { Run(); };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ups, something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Run()
        {
            dataSource.Images?.Dispose();
            dataSource.LoadNext();
            ColorImage.Source = dataSource.Images.ColorImage?.Flip(FlipMode.Y).ToBitmapSource();
            DepthImage.Source = dataSource.Images.DepthImage?.Flip(FlipMode.Y).ToBitmapSource();
        }

        private void ChangeSaveDirectory(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var enable = false;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directory = fbd.SelectedPath;
                enable = true;

            }
            foreach (var b in BottomButtons.Children)
            {
                (b as Button).IsEnabled = enable;
                DataSourceSelector.IsEnabled = enable;
            }
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Q:
                    TakeImage(Unknown, null);
                    break;
                case Key.W:
                    TakeImage(Correct, null);
                    break;
                case Key.E:
                    TakeImage(Wrong, null);
                    break;
                case Key.R:
                    TakeImage(Slouch, null);
                    break;
                case Key.Left:
                    TakeImage(LeanLeft, null);
                    break;
                case Key.Right:
                    TakeImage(LeanRight, null);
                    break;
                case Key.Up:
                    TakeImage(LeanForward, null);
                    break;
                case Key.Down:
                    TakeImage(LeanBackward, null);
                    break;
                case Key.Escape:
                    Application.Current.Shutdown(); break;

            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ChangeSaveDirectory(this, null);
        }

        private void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            dataSource?.Stop();
        }
    }
}