using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public interface ISaveImage
    {
        public Task SaveImg(IFormFileCollection images, IWebHostEnvironment appEnvironment, Lot lot);
    }
}