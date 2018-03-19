using System.Diagnostics;
using System.Reflection;
using System.Resources;
using Caliburn.Micro;

namespace SpineHero.ViewModels.DialogWindows
{
    public class HowToSitCorrectlyViewModel : Screen
    {
        private ResourceManager translation;

        public HowToSitCorrectlyViewModel()
        {
            translation = new ResourceManager("SpineHero.Views.DialogWindows.Dialog", Assembly.GetExecutingAssembly());
            DisplayName = translation.GetString("HowToSitTitle");
        }

        public void ReadMore()
        {
            var url = translation.GetString("HowToSitBlogUrl");
            Process.Start(new ProcessStartInfo(url));
        }
    }
}