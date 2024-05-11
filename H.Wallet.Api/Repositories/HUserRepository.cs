using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using H.Wallet.Api.Data;
using H.Wallet.Api.Models;

namespace H.Wallet.Api.Repositories;

public interface IHUserRepository
{
    public Task Add(HUser entity);
    public Task<HUser?> Get(Expression<Func<HUser, bool>> condition);
    public Task<List<T>> GetAll<T>(Expression<Func<HUser, T>> feature, Expression<Func<HUser, bool>> condition);
    public Task<List<HUser>> GetAll(Expression<Func<HUser, bool>> condition);
    public Task<List<HUser>> GetAll();
    public Task Remove(HUser entity);
}


public class HUserRepository : BaseRepository<HUser>, IHUserRepository
{
    public HUserRepository(DataContext context)
    {
        _context = context;
    }

    public override async Task<HUser?> Get(Expression<Func<HUser, bool>> condition)
    {
        return await _context.HUsers.Include(h => h.Wallets).FirstOrDefaultAsync(condition);
    }
    
    public override async Task<List<HUser>> GetAll(Expression<Func<HUser, bool>> condition)
    {
        return await _context.HUsers.Where(condition).Include(h => h.Wallets).ToListAsync();
    }
    
    public override async Task<List<T>> GetAll<T>(Expression<Func<HUser, T>> feature, Expression<Func<HUser, bool>> condition)
    {
        return await _context.HUsers.Where(condition).Select(feature).ToListAsync();
    }

    public override async Task<List<HUser>> GetAll()
    {
        return await _context.HUsers.Include(h => h.Wallets).ToListAsync();
    }
}