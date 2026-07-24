namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// سرویس گلوبال آپلود و مدیریت فایل
    /// استفاده: @inject IFileUploadService FileUpload
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// آپلود فایل و برگرداندن GUID
        /// </summary>
        Task<UploadResult?> UploadAsync(string base64Data, string contentType);
        
        /// <summary>
        /// دریافت فایل با GUID (برگرداندن Base64)
        /// </summary>
        Task<FileData?> GetFileAsync(string fileId);
        
        /// <summary>
        /// آپلود تصویر از data URL
        /// </summary>
        Task<string?> UploadImageAsync(string dataUrl);
        
        /// <summary>
        /// حذف فایل با GUID
        /// </summary>
        Task<bool> DeleteFileAsync(string fileId);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly ICommunicationService _comm;

        public FileUploadService(ICommunicationService comm)
        {
            _comm = comm;
        }

        public async Task<UploadResult?> UploadAsync(string base64Data, string contentType)
        {
            var request = new
            {
                Base64Data = base64Data,
                FileName = $"{Guid.NewGuid()}.bin",
                ContentType = contentType
            };

            return await _comm.PostAsync<UploadResult>("api/upload", request);
        }

        public async Task<FileData?> GetFileAsync(string fileId)
        {
            try
            {
                // دریافت فایل از سرور (برگرداندن Base64)
                var response = await _comm.GetAsync<FileResponse>($"api/upload/{fileId}");
                return response?.data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> UploadImageAsync(string dataUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(dataUrl) || !dataUrl.StartsWith("data:"))
                    return null;

                var base64 = dataUrl.Split(',')[1];
                var contentType = dataUrl.Split(';')[0].Split(':')[1];

                var result = await UploadAsync(base64, contentType);
                return result?.data?.id; // برگرداندن GUID
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            return await _comm.DeleteAsync($"api/upload/{fileId}");
        }
    }

    public class UploadResult
    {
        public bool success { get; set; }
        public UploadData? data { get; set; }
    }

    public class UploadData
    {
        public string id { get; set; } = "";
        public string fileName { get; set; } = "";
        public string url { get; set; } = "";
        public string contentType { get; set; } = "";
    }

    public class FileResponse
    {
        public bool success { get; set; }
        public FileData? data { get; set; }
    }

    public class FileData
    {
        public string id { get; set; } = "";
        public string base64 { get; set; } = "";
        public string contentType { get; set; } = "";
    }
}
