using ComicPrices.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicPrices.Data
{
    public interface IComicsDbContext
    {
        DbSet<Comic> Comics { get; set; }
        DbSet<Price> Prices { get; set; }
        DbSet<Seller> Sellers { get; set; }

        // If SaveChanges is being used
        int SaveChanges();
    }

    public class ComicsDbContext : DbContext, IComicsDbContext
    {
        public ComicsDbContext(DbContextOptions<ComicsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comic> Comics { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Seller> Sellers { get; set; }
    }
}