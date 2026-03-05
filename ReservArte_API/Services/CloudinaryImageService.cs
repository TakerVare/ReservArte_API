
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

       public CloudinaryImageService(IConfiguration configuration)
       {
           var cloudName = configuration["CloudinarySettings:CloudName"];
           var apiKey = configuration["CloudinarySettings:ApiKey"];
           var apiSecret = configuration["CloudinarySettings:ApiSecret"];
           var account = new Account(cloudName, apiKey, apiSecret);
           _cloudinary = new Cloudinary(account);
       }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file.Length <= 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
           {
               File = new FileDescription(file.FileName, stream),
               Transformation = new Transformation().Width(400).Height(400).Crop("fill")
               //Transformation = new Transformation().Width(300).Crop("scale").Chain().Effect("cartoonify")
               //Effect(vignette",20), Effect("remove_background", 0.5), Effect("sharpen", 50)  
           };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }
    }
}