namespace CRM.WebBlazor.Service
{
    public interface IFileService
    {
        Task<string> ReadFileAsync(string path);
    }
}
