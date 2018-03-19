using Caliburn.Micro;

namespace SpineHero.ViewModels.DialogWindows
{
    class WelcomeViewModel : Screen
    {
        public WelcomeViewModel()
        {
            DisplayName = "Welcome to Spine Hero";
        }

        public void Confirm()
        {
            TryClose();
            Properties.Settings.Default.DisplayWelcomeScreenOnStart = false;
        }
    }
}
