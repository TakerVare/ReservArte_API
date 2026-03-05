namespace ReservArte_API.Services.Interfaces
{
   public interface IImageService
   {
       Task<string> UploadImageAsync(IFormFile file);
      // Task DeleteImageAsync(string publicId);
   }
}
