using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Models.Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
    }
}
