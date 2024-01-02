namespace HtmlToPdfService.Services
{
    public interface IStorageService
    {
        Task<string> PutAsync(string fileName, Stream stream);
        Task<Stream> GetAsync(string path);
        Task DeleteAsync(string path);
    }
}
