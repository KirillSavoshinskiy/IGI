﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IGI.Models;

namespace IGI.Data
{
    public class ApplicationContext: IdentityDbContext<User>
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Img> Images { get; set; } 
        public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
        {
           // Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        // protected override void OnModelCreating(ModelBuilder builder)
        // {
        //     builder.Entity<Lot>().HasMany(i => i.Images).WithOne();
        // }
    }
}