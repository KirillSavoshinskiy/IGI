using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class SaveImage : ISaveImage
    {
        IServiceProvider _serviceProvider;

        public SaveImage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SaveImg(IFormFileCollection images, IWebHostEnvironment appEnvironment, Lot lot)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                await using (var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    foreach (var image in images)
                    {
                        var path = "/Files/" + image.FileName;
                        await using (var fileStream =
                            new FileStream(appEnvironment.WebRootPath + path, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var img = new Img
                        {
                            ImgPath = path,
                            Name = image.Name,
                            LotId = lot.Id
                        };
                        await context.Images.AddAsync(img);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}