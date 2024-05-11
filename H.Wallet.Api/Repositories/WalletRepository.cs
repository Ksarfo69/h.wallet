using System.Linq.Expressions;
using H.Wallet.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace H.Wallet.Api.Repositories;


public interface IWalletRepository
{
    public Task Add(Models.Wallet entity);
    public Task<Models.Wallet?> Get(Expression<Func<Models.Wallet, bool>> condition);
    public Task<List<Models.Wallet>> GetAll(Expression<Func<Models.Wallet, bool>> condition);
    public Task<List<Models.Wallet>> GetAll();
    public Task Remove(Models.Wallet entity);
}


public class WalletRepository : BaseRepository<Models.Wallet>, IWalletRepository
{
    public WalletRepository(DataContext context)
    {
        _context = context;
    }
    
    public override async Task<Models.Wallet?> Get(Expression<Func<Models.Wallet, bool>> condition)
    {
        return await _context.Wallets.Include(w => w.Owner).FirstOrDefaultAsync(condition);
    }
    
    public override async Task<List<Models.Wallet>> GetAll(Expression<Func<Models.Wallet, bool>> condition)
    {
        return await _context.Wallets.Where(condition).Include(w => w.Owner).ToListAsync();
    }

    public override async Task<List<Models.Wallet>> GetAll()
    {
        return await _context.Wallets.Include(w => w.Owner).ToListAsync();
    }
}