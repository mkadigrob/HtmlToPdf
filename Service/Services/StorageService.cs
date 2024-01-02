
using HtmlToPdfService.Helpers;

namespace HtmlToPdfService.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _rootPath;

        public StorageService(IConfiguration configuration)
        {
            _rootPath = configuration.GetStoragePath();
        }

        public async Task<string> PutAsync(string fileName, Stream stream)
        {
            var path = Path.Combine(_rootPath, fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
            return path;
        }

        public async Task<Stream> GetAsync(string path)
        {
            return await Task.Run<Stream>(() => File.OpenRead(path));
        }

        public async Task DeleteAsync(string path)
        {
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }
    }
}
