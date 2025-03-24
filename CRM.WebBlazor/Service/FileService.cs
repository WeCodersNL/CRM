namespace CRM.WebBlazor.Service
{
    public class FileService : IFileService
    {
        public async Task<string> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }
            return await File.ReadAllTextAsync(path);
        }
    }
}
