using System.Threading.Tasks;

namespace ForeScore
{
    public interface IFileHelper
    {
        Task<string> LoadLocalFileAsync(string filename);
        Task<bool> SaveLocalFileAsync(string filename, string data);

        Task<bool> DeleteLocalFileAsync(string filename);

        string GetNameWithPath(string filename);
    }
}
