using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Windows;
using Caliburn.Micro;
using Infralution.Localization.Wpf;
using Newtonsoft.Json;
using SpineHero.Utils;
using SpineHero.Utils.CloudStorage;
using SpineHero.Utils.Logging;

namespace SpineHero.ViewModels.MainMenuItems
{
    class FeedbackViewModel : Screen, IMainMenuItem
    {
        public ICloudStorage CloudStorage { get; set; }
        private string feedbackText;
        private readonly ILogger log = Logger.GetLogger<FeedbackViewModel>();
        private string sendBtnText;
        private ResourceManager translation;

        public FeedbackViewModel()
        {
            CloudStorage = new ElasticEmailApi();
            translation = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = translation.GetString("Feedback");
            SendBtnText = translation.GetString("FeedbackSend");
            CultureManager.UICultureChanged += (o, e) =>
            {
                DisplayName = translation.GetString("Feedback");
                SendBtnText = translation.GetString("FeedbackSend");
            };
        }

        public string SendBtnText
        {
            get { return sendBtnText; }
            set
            {
                sendBtnText = value;
                NotifyOfPropertyChange(() => SendBtnText);
            }
        }

        public string FeedbackText
        {
            get { return feedbackText; }
            set
            {
                feedbackText = value;
                NotifyOfPropertyChange(() => FeedbackText);
            }
        }

        public async void Send()
        {
            if (string.IsNullOrWhiteSpace(this.FeedbackText))
            {
                MessageBox.Show(translation.GetString("FeedbackEmptyMessage"));
                return;
            }

            try
            {
                SendBtnText = translation.GetString("FeedbackSending");
                await CloudStorage.SaveData("Feedback", JsonConvert.SerializeObject(new Dictionary<string, object>()
                {
                    {"Machine", Environment.MachineName},
                    {"OS", Environment.OSVersion.ToString()},
                    {"Feedback", FeedbackText}
                }));

                FeedbackText = string.Empty;
                MessageBox.Show(translation.GetString("FeedbackSuccess"), translation.GetString("Sending feedback"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (CloudStorageException e)
            {
                log.Error(e);
                MessageBox.Show(translation.GetString("FeedbackFail"), translation.GetString("Sending feedback"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                SendBtnText = translation.GetString("FeedbackSend");
            }
        }
    }
}
