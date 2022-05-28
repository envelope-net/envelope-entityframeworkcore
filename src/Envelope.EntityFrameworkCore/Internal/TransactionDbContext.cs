using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Envelope.Transactions;
using Envelope.Transactions.Internal;
using System.Data;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Internal;

internal class TransactionDbContext : TransactionContext, ITransactionContext, IDisposable, IAsyncDisposable
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IDbContextCache _dbContextCache;

	protected internal TransactionDbContext(IServiceProvider serviceProvider, Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		: base(configureBehavior, configure)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_dbContextCache = new DbContextCache(_serviceProvider);
	}

	/// <summary>
	/// Creates new not cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="newDbContextTransaction">Created DB transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	public TContext CreateNewDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> _dbContextCache.CreateNewDbContextWithNewTransaction<TContext>(
			out newDbContextTransaction,
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new not cached <see cref="DbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	//public TContext CreateNewDbContextWithExistingTransaction<TContext>(
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : DbContext
	//{
	//	var context = 
	//		_dbContextCache.CreateNewDbContextWithExistingTransaction<TContext>(
	//			dbContextTransaction,
	//			commandQueryName,
	//			idCommandQuery);

	//	if (TryAddItem(dbContextTransaction.GetType().FullName!, dbContextTransaction))
	//	{
	//		OnCommitted((transactionContext, cancellationToken) => dbContextTransaction.CommitAsync(cancellationToken));
	//		OnCommitted((transactionContext) => dbContextTransaction.Commit());
	//		OnRollback((transactionContext, cancellationToken) => dbContextTransaction.RollbackAsync(cancellationToken));
	//		OnRollback((transactionContext) => dbContextTransaction.Rollback());
	//		OnDisposed(transactionContext => dbContextTransaction.DisposeAsync());
	//	}

	//	return context;
	//}

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	public TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> _dbContextCache.GetOrCreateDbContextWithNewTransaction<TContext>(
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new cached <see cref="DbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	//public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : DbContext
	//	=> GetOrCreateDbContextWithExistingTransaction<TContext>(
	//		typeof(TContext).FullName!,
	//		dbContextTransaction,
	//		commandQueryName,
	//		idCommandQuery);

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="DbContext"/>.FullName </param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	public TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		string key,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> _dbContextCache.GetOrCreateDbContextWithNewTransaction<TContext>(
			key,
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new cached <see cref="DbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="key">Cache key. Default is <see cref="DbContext"/>.FullName </param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	//public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
	//	string key,
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : DbContext
	//{
	//	if (string.IsNullOrWhiteSpace(key))
	//		throw new ArgumentNullException(nameof(key));

	//	var context =
	//		_dbContextCache.GetOrCreateDbContextWithExistingTransaction<TContext>(
	//			key,
	//			dbContextTransaction,
	//			commandQueryName,
	//			idCommandQuery);

	//	if (TryAddItem(key, dbContextTransaction))
	//	{
	//		OnCommitted((transactionContext, cancellationToken) => dbContextTransaction.CommitAsync(cancellationToken));
	//		OnCommitted((transactionContext) => dbContextTransaction.Commit());
	//		OnRollback((transactionContext, cancellationToken) => dbContextTransaction.RollbackAsync(cancellationToken));
	//		OnRollback((transactionContext) => dbContextTransaction.Rollback());
	//		OnDisposed(transactionContext => dbContextTransaction.DisposeAsync());
	//	}

	//	return context;
	//}

	/// <summary>
	/// Creates new not cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="newDbContextTransaction">Created DB transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	public TContext CreateNewIDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> _dbContextCache.CreateNewIDbContextWithNewTransaction<TContext>(
			out newDbContextTransaction,
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new not cached <see cref="IDbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	//public TContext CreateNewIDbContextWithExistingTransaction<TContext>(
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : IDbContext
	//{
	//	var context =
	//		_dbContextCache.CreateNewIDbContextWithExistingTransaction<TContext>(
	//			dbContextTransaction,
	//			commandQueryName,
	//			idCommandQuery);

	//	if (TryAddItem(dbContextTransaction.GetType().FullName!, dbContextTransaction))
	//	{
	//		OnCommitted((transactionContext, cancellationToken) => dbContextTransaction.CommitAsync(cancellationToken));
	//		OnCommitted((transactionContext) => dbContextTransaction.Commit());
	//		OnRollback((transactionContext, cancellationToken) => dbContextTransaction.RollbackAsync(cancellationToken));
	//		OnRollback((transactionContext) => dbContextTransaction.Rollback());
	//		OnDisposed(transactionContext => dbContextTransaction.DisposeAsync());
	//	}

	//	return context;
	//}

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	public TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> _dbContextCache.GetOrCreateIDbContextWithNewTransaction<TContext>(
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new cached <see cref="IDbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	//public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : IDbContext
	//	=> GetOrCreateIDbContextWithExistingTransaction<TContext>(
	//		typeof(TContext).FullName!,
	//		dbContextTransaction,
	//		commandQueryName,
	//		idCommandQuery);

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="IDbContext"/>.FullName </param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	public TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		string key,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> _dbContextCache.GetOrCreateIDbContextWithNewTransaction<TContext>(
			key,
			this,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	///// <summary>
	///// Creates new cached <see cref="IDbContext"/> with and reuse DB transaction
	///// </summary>
	///// <param name="dbContextTransaction">Reuse DB transaction</param>
	///// <param name="key">Cache key. Default is <see cref="IDbContext"/>.FullName </param>
	///// <param name="commandQueryName"></param>
	///// <param name="idCommandQuery"></param>
	///// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	//public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
	//	string key,
	//	IDbContextTransaction dbContextTransaction,
	//	string? commandQueryName = null,
	//	Guid? idCommandQuery = null)
	//	where TContext : IDbContext
	//{
	//	if (string.IsNullOrWhiteSpace(key))
	//		throw new ArgumentNullException(nameof(key));

	//	var context =
	//		_dbContextCache.GetOrCreateIDbContextWithExistingTransaction<TContext>(
	//			key,
	//			dbContextTransaction,
	//			commandQueryName,
	//			idCommandQuery);

	//	if (TryAddItem(key, dbContextTransaction))
	//	{
	//		OnCommitted((transactionContext, cancellationToken) => dbContextTransaction.CommitAsync(cancellationToken));
	//		OnCommitted((transactionContext) => dbContextTransaction.Commit());
	//		OnRollback((transactionContext, cancellationToken) => dbContextTransaction.RollbackAsync(cancellationToken));
	//		OnRollback((transactionContext) => dbContextTransaction.Rollback());
	//		OnDisposed(transactionContext => dbContextTransaction.DisposeAsync());
	//	}

	//	return context;
	//}
}
