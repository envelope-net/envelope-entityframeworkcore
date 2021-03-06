using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public class DbContextProvider<TIdentity> : IDbContextProvider
	where TIdentity : struct
{
	protected IDbContextCache DbContextCache { get; private set; }

	public DbContextProvider(IDbContextCache dbContextCache)
	{
		DbContextCache = dbContextCache ?? throw new ArgumentNullException(nameof(dbContextCache));
	}

	public TContext GetOrCreateDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
		where TContext : IDbContext
		=> DbContextCache.GetOrCreateIDbContextWithoutTransaction<TContext>(externalDbConnection, connectionString, null, null);

	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(IDbContextTransaction dbContextTransaction, ITransactionManager? transactionManager = null)
		where TContext : IDbContext
		=> DbContextCache.GetOrCreateIDbContextWithExistingTransaction<TContext>(dbContextTransaction, transactionManager, null, null);

	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(DbTransaction dbTransaction, ITransactionManager? transactionManager = null)
		where TContext : IDbContext
		=> DbContextCache.GetOrCreateIDbContextWithExistingTransaction<TContext>(dbTransaction, transactionManager, null, null);

	public TContext GetOrCreateDbContextWithNewTransaction<TContext>(ITransactionManager? transactionManager = null)
		where TContext : IDbContext
	{
		if (transactionManager is TransactionDbContext<TIdentity> transactionDbContext)
			return transactionDbContext.GetOrCreateIDbContextWithNewTransaction<TContext>(null, null, null, null, null);
		else
			return DbContextCache.GetOrCreateIDbContextWithNewTransaction<TContext>(transactionManager, null, null, null, null, null);
	}
}
