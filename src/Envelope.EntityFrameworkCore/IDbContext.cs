using Envelope.Database;
using Envelope.EntityFrameworkCore.QueryCache;
using Envelope.Trace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Envelope.EntityFrameworkCore;

public interface IDbContext : IDisposable, IAsyncDisposable
{
	DbConnection DbConnection { get; }
	IDbContextTransaction? DbContextTransaction { get; }
	DbTransaction? DbTransaction { get; }
	DbContextId ContextId { get; }
	ChangeTracker ChangeTracker { get; }
	DatabaseFacade Database { get; }
	string DBConnectionString { get; }
	string? CommandQueryName { get; }
	Guid? IdCommandQuery { get; }

	int Save(
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	int Save(
		ITraceInfo traceInfo,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	int Save(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	int Save(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	int Save(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		SaveOptions? options,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	Task<int> SaveAsync(
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	Task<int> SaveAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		SaveOptions? options,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0);

	void ConfigureQueryCacheManager(Action<QueryCacheManager> configure, bool force);

	void EnableQueryCacheManager();

	void SetDbTransaction(
		IDbContextTransaction? existingDbContextTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel);

	void SetDbTransaction(
		DbTransaction? existingTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel);
}
