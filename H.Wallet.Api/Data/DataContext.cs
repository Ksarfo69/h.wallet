using H.Wallet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace H.Wallet.Api.Data;

public class DataContext : DbContext
{
    public DbSet<HUser> HUsers { get; set; }
    public DbSet<Models.Wallet> Wallets { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
}