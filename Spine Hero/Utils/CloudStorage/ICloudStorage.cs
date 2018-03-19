using System.Threading.Tasks;

namespace SpineHero.Utils.CloudStorage
{
    public interface ICloudStorage
    {
        Task<int> SaveData(string type, string data);
    }
}