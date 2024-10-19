using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Data;

public class UnitOfWork : IUnitOfWork
{
	private readonly DbContext context;

    public UnitOfWork(AppDbContext context)
    {
        this.context = context;
    }

    public void Commit()
	{
		context.SaveChanges();
	}

	public async Task CommitAsync()
	{
		await context.SaveChangesAsync();
	}
}
