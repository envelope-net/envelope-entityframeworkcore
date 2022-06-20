using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Envelope.Transactions;
using System.Data;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public interface IDbContextCache
{
	/// <summary>
	/// Creates new not cached <see cref="DbContext"/> without DB transaction
	/// </summary>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> without transaction</returns>
	TContext CreateNewDbContext<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new not cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="newDbContextTransaction">Created DB transaction</param>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	TContext CreateNewDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new not cached <see cref="DbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	TContext CreateNewDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> without DB transaction
	/// </summary>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> without transaction</returns>
	TContext GetOrCreateDbContextWithoutTransaction<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> without DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="DbContext"/>.FullName </param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> without transaction</returns>
	TContext GetOrCreateDbContextWithoutTransaction<TContext>(
		string key,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with new DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="DbContext"/>.FullName </param>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with new DB transaction</returns>
	TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		string key,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new cached <see cref="DbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="key">Cache key. Default is <see cref="DbContext"/>.FullName </param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="DbContext"/> with existing DB transaction</returns>
	TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext;

	/// <summary>
	/// Creates new not cached <see cref="IDbContext"/> without DB transaction
	/// </summary>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> without transaction</returns>
	TContext CreateNewIDbContext<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new not cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="newDbContextTransaction">Created DB transaction</param>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	TContext CreateNewIDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new not cached <see cref="IDbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	TContext CreateNewIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> without DB transaction
	/// </summary>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> without transaction</returns>
	TContext GetOrCreateIDbContextWithoutTransaction<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> without DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="IDbContext"/>.FullName </param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> without transaction</returns>
	TContext GetOrCreateIDbContextWithoutTransaction<TContext>(
		string key,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with new DB transaction
	/// </summary>
	/// <param name="key">Cache key. Default is <see cref="IDbContext"/>.FullName </param>
	/// <param name="transactionManager">Register created transaction</param>
	/// <param name="transactionIsolationLevel">Create DB transaction with <see cref="IsolationLevel"/></param>
	/// <param name="externalDbConnection">Use external DB connection</param>
	/// <param name="connectionString">If externalDbConnection is null, use connection string</param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with new DB transaction</returns>
	TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		string key,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;

	/// <summary>
	/// Creates new cached <see cref="IDbContext"/> with and reuse DB transaction
	/// </summary>
	/// <param name="dbContextTransaction">Reuse DB transaction</param>
	/// <param name="key">Cache key. Default is <see cref="IDbContext"/>.FullName </param>
	/// <param name="commandQueryName"></param>
	/// <param name="idCommandQuery"></param>
	/// <returns>New <see cref="IDbContext"/> with existing DB transaction</returns>
	TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext;
}
