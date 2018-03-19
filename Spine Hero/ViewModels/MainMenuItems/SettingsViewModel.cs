using Caliburn.Micro;
using DirectShowLib;
using Infralution.Localization.Wpf;
using SpineHero.Common.Logging;
using SpineHero.Model;
using SpineHero.Monitoring.DataSources;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.Utils.Logging;
using SpineHero.ViewModels.DialogWindows;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace SpineHero.ViewModels.MainMenuItems
{
    public sealed class SettingsViewModel : Screen, IMainMenuItem
    {
        private readonly IWindowManager windowManager;
        private readonly IPostureMonitoringManager postureAnalysis;
        private readonly CalibrationWindowViewModel calibrationViewModel;
        private readonly ILogger log = Logger.GetLogger<SettingsViewModel>();
        private CultureInfo selectedLanguage;
        private static readonly CultureInfo cultureEN = new CultureInfo("en");
        private static readonly CultureInfo cultureSK = new CultureInfo("sk");

        public SettingsViewModel(IEventAggregator ea, IWindowManager wm, IPostureMonitoringManager pam, CalibrationWindowViewModel calibVM)
        {
            ResourceManager rm = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = rm.GetString("Settings");
            ea.Subscribe(this);
            windowManager = wm;
            postureAnalysis = pam;
            calibrationViewModel = calibVM;
            selectedLanguage = CultureManager.UICulture;
            CultureManager.UICultureChanged += (o, e) =>
            {
                DisplayName = rm.GetString("Settings");
            };
        }

        public bool AutoStart
        {
            get { return AutoStartup.AutoStart; }
            set
            {
                AutoStartup.AutoStart = value;
                NotifyOfPropertyChange(() => AutoStart);
            }
        }

        public string ApplicationVersionNumber => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public bool UseDepthCamera
        {
            get { return Settings.Default.UseDepthCamera; }
            set
            {
                Settings.Default.UseDepthCamera = value;
                NotifyOfPropertyChange(() => UseDepthCamera);
            }
        }

        public List<DsDevice> CameraList => WebCameraDataSource.GetListOfCameras();

        [LogMethodCall]
        public async void Recalibrate()
        {
            var run = postureAnalysis.IsMonitoring;
            if (run) await Task.Run(() => postureAnalysis.StopMonitoring(false));
            windowManager.ShowDialog(calibrationViewModel);
            if (run) await Task.Run(() => postureAnalysis.StartMonitoring());
        }

        public void HowToSitCorrectly()
        {
            log.LogEvent("How to sit correctly");
            windowManager.ShowDialog(new HowToSitCorrectlyViewModel());
        }

        public List<string> LanguageCatalog
        {
            get
            {
                return new List<string> { cultureEN.EnglishName, cultureSK.EnglishName };
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return selectedLanguage.EnglishName;
            }
            set
            {
                if (value.Equals(cultureSK.EnglishName))
                {
                    selectedLanguage = cultureSK;
                }
                else
                {
                    selectedLanguage = cultureEN;
                }
                CultureManager.UICulture = selectedLanguage;
                Settings.Default.AppLanguage = selectedLanguage.ToString();
                NotifyOfPropertyChange(() => SelectedLanguage);
            }
        }
    }
}