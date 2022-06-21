using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Envelope.EntityFrameworkCore.Internal;
using Envelope.Model;
using Envelope.Services;
using Envelope.Transactions;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public abstract class DbServiceContext<TEntity> : ServiceBase<TEntity>, IService<TEntity>
	where TEntity : IEntity
{
	protected IDbContextCache DbContextCache { get; private set; }

	public DbServiceContext(IDbContextCache dbContextCache, ILogger logger)
		: base(logger)
	{
		DbContextCache = dbContextCache ?? throw new ArgumentNullException(nameof(dbContextCache));
	}

	protected TContext GetOrCreateDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
		where TContext : IDbContext
		=> DbContextCache.GetOrCreateIDbContextWithoutTransaction<TContext>(externalDbConnection, connectionString, null, null);

	protected TContext GetOrCreateDbContextWithExistingTransaction<TContext>(IDbContextTransaction dbContextTransaction)
		where TContext : IDbContext
		=> DbContextCache.GetOrCreateIDbContextWithExistingTransaction<TContext>(dbContextTransaction, null, null);

	protected TContext GetOrCreateDbContextWithNewTransaction<TContext>(ITransactionManager? transactionManager = null)
		where TContext : IDbContext
	{
		if (transactionManager is TransactionDbContext transactionDbContext)
			return transactionDbContext.GetOrCreateIDbContextWithNewTransaction<TContext>(null, null, null, null, null);
		else
			return DbContextCache.GetOrCreateIDbContextWithNewTransaction<TContext>(transactionManager, null, null, null, null, null);
	}
}
