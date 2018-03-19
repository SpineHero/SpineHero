using SpineHero.Common.Resources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Utils.Extentions
{
    public static class PostureHelper
    {
        public static string GetImageRepresentation(this Posture posture)
        {
            var name = posture.ToString();
            return ResourceHelper.GetResourcePath(@"Resources\Images\StickBoy\" + name + ".png");
        }
    }
}