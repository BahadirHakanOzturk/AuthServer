using AuthServer.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthServer.Data.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
	private readonly AppDbContext context;
	private readonly DbSet<TEntity> dbSet;

	public GenericRepository(AppDbContext context)
	{
		this.context = context;
		dbSet = context.Set<TEntity>();
	}

	public async Task AddAsync(TEntity entity)
	{
		await dbSet.AddAsync(entity);
	}

	public async Task<IEnumerable<TEntity>> GetAllAsync()
	{
		return await dbSet.ToListAsync();
	}

	public async Task<TEntity> GetByIdAsync(string id)
	{
		var entity = await dbSet.FindAsync(Guid.Parse(id));
		if (entity != null)
		{
			context.Entry(entity).State = EntityState.Detached;
		}
		return entity;
	}

	public void Remove(TEntity entity)
	{
		dbSet.Remove(entity); 
	}

	public TEntity Update(TEntity entity)
	{
		context.Entry(entity).State = EntityState.Modified;
		return entity;
	}

	public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
	{
		return dbSet.Where(predicate);
	}
}
