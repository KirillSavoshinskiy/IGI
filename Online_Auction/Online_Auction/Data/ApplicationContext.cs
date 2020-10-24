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
        public DbSet<Comment> Comments { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        // protected override void OnModelCreating(ModelBuilder builder)
        // {
        //     builder.Entity<Img>().HasOne(l => l.Lot)
        //         .WithMany(i => i.Images)
        //         .HasForeignKey(k => k.LotId).OnDelete(DeleteBehavior.Cascade);
        // }
    }
}