using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class SaveImage: ISaveImage
    {
        public async Task SaveImg(IFormFileCollection images, ApplicationContext context,
            IWebHostEnvironment appEnvironment, Lot lot)
        {
            foreach (var image in images)
            { 
                var path = "/Files/" + image.FileName; 
                await using (var fileStream = new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                var img = new Img
                {
                    ImgPath = path,
                    Name = image.Name,
                    LotId = lot.Id
                };
                context.Images.Add(img);
                await context.SaveChangesAsync();
            }
        }
    }
}