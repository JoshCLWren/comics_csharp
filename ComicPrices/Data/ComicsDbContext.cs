using ComicPrices.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicPrices.Data;

public class ComicsDbContext : DbContext
{
    public ComicsDbContext(DbContextOptions<ComicsDbContext> options) : base(options)
    {
    }

    public DbSet<Comic> Comics { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    
}