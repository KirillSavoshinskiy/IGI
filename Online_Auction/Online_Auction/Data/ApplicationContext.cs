using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Data
{
    public class ApplicationContext: IdentityDbContext<User>
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Img> Images { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}