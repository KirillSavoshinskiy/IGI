using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IGI.Data;
using IGI.Models;

namespace IGI.Services
{
    public class SaveImage : ISaveImage
    {
        ApplicationContext _context;
        public SaveImage(ApplicationContext context)
        {
            _context = context;
        }

        public async Task SaveImg(IFormFileCollection images, IWebHostEnvironment appEnvironment, Lot lot)
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
                await _context.Images.AddAsync(img);
                await _context.SaveChangesAsync();
            }
        }
    }
}