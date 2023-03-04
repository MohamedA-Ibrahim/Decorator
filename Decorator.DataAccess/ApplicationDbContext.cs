using Decorator.DataAccess.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace Decorator.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductDimension> ProductDimensions { get; set; }

        public DbSet<CustomOrder> CustomOrders { get; set; }

        public DbSet<CustomOrderItem> CustomOrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
                                     optionsBuilder.EnableSensitiveDataLogging()
                                     .LogTo(message => Debug.WriteLine(message));
    }

}