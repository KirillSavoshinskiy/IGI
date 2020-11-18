﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using IGI.Data;
using IGI.Models;

namespace IGI.Services
{
    public interface ISaveImage
    {
        public Task SaveImg(IFormFileCollection images, IWebHostEnvironment appEnvironment, Lot lot);
    }
}