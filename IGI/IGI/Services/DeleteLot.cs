using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IGI.Data;
using IGI.Models;

namespace IGI.Services
{
    public class DeleteLot : IDeleteLot
    {
        private ApplicationContext _context;
        public DeleteLot(ApplicationContext context)
        {
            _context = context;
        }

        public async void Delete(Lot lot)
        {
            _context.Lots.Remove(lot);
            await _context.SaveChangesAsync();
        }
    }
}