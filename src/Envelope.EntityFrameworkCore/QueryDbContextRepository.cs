using Microsoft.Extensions.Logging;
using Envelope.Model;
using Envelope.Services;

namespace Envelope.EntityFrameworkCore;

public abstract class QueryDbContextRepository<TDbContext, TEntity> : QueryRepositoryBase<TEntity>, IQueryRepository<TEntity>
	where TDbContext : IDbContext
	where TEntity : IQueryEntity
{
	protected TDbContext DbContext { get; private set; }

	public QueryDbContextRepository(TDbContext dbContext, ILogger logger)
		: base(logger)
	{
		DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public void SetDbContext(TDbContext dbContext)
	{
		DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}
}
