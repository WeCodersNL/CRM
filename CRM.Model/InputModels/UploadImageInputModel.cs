namespace CRM.Model.InputModels
{
    public class UploadImageInputModel()
    {
        public required string ImageFormat { get; set; }
        public required string ImageBase64 { get; set; }
        public required string Container { get; set; }
    }
}
