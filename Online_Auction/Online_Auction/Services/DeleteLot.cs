﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Auction.Data;
using Online_Auction.Models;

namespace Online_Auction.Services
{
    public class DeleteLot : IDeleteLot
    {
        IServiceProvider _serviceProvider;

        public DeleteLot(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async void Delete(Lot lot)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    context.Lots.Remove(lot);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}