using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;

namespace SpineHero.ViewModels
{
    public interface IShell
    {
        void ShowNotification(string title, string text, BalloonIcon icon);

        void ShowNotification(string title, string text, PopupAnimation animation, int? timeout);
    }
}