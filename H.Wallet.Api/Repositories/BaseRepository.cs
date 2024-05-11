using System.Linq.Expressions;
using H.Wallet.Api.Data;
using H.Wallet.Api.Models;

namespace H.Wallet.Api.Repositories;

public abstract class BaseRepository<T> where T : BaseModel
{
    protected DataContext _context { get; set; }

    public async Task Add(T entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    
    public abstract Task<T?> Get(Expression<Func<T, bool>> condition);
    
    public abstract Task<List<T>> GetAll(Expression<Func<T, bool>> condition);

    public abstract Task<List<R>> GetAll<R>(Expression<Func<T, R>> feature, Expression<Func<T, bool>> condition);
    
    public abstract Task<List<T>> GetAll();
    
    public async Task Remove(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }
}