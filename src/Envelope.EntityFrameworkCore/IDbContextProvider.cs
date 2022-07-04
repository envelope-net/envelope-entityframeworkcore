using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public interface IDbContextProvider
{
	TContext GetOrCreateDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
		where TContext : IDbContext;

	TContext GetOrCreateDbContextWithExistingTransaction<TContext>(IDbContextTransaction dbContextTransaction, ITransactionManager? transactionManager = null)
		where TContext : IDbContext;

	TContext GetOrCreateDbContextWithExistingTransaction<TContext>(DbTransaction dbTransaction, ITransactionManager? transactionManager = null)
		where TContext : IDbContext;

	TContext GetOrCreateDbContextWithNewTransaction<TContext>(ITransactionManager transactionManager)
		where TContext : IDbContext;
}
