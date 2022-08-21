using Envelope.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public static partial class DbContextFactory
{
	public static TContext CreateNewDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbContextTransaction existingDbContextTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (existingDbContextTransaction == null)
			throw new ArgumentNullException(nameof(existingDbContextTransaction));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(
				existingDbContextTransaction.GetDbTransaction().Connection,
				null);
		}

		return SetDbTransaction(dbContext, existingDbContextTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);
	}

	public static TContext CreateNewDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		DbTransaction existingDbTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (existingDbTransaction == null)
			throw new ArgumentNullException(nameof(existingDbTransaction));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(
				existingDbTransaction.Connection,
				null);
		}

		return SetDbTransaction(dbContext, existingDbTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);
	}

	public static TContext CreateNewDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbTransactionFactory dbTransactionFactory,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (dbTransactionFactory == null)
			throw new ArgumentNullException(nameof(dbTransactionFactory));

		dbTransactionFactory.Initialize();

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(dbTransactionFactory.DbConnection, null);
		}

		var dbTransaction = dbTransactionFactory.GetOrBeginTransaction();
		return SetDbTransaction(dbContext, dbTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);
	}

	public static async Task<(TContext dbContext, IDbContextTransaction? newDbContextTransaction)> CreateNewDbContextAsync<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbTransactionFactory dbTransactionFactory,
		string? commandQueryName = null,
		Guid? idCommandQuery = null,
		CancellationToken cancellationToken = default)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (dbTransactionFactory == null)
			throw new ArgumentNullException(nameof(dbTransactionFactory));

		await dbTransactionFactory.InitializeAsync();

		var context = serviceProvider.GetRequiredService<TContext>();
		if (context is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(dbTransactionFactory.DbConnection, null);
		}

		var dbTransaction = await dbTransactionFactory.GetOrBeginTransactionAsync(cancellationToken);
		SetDbTransaction(context, dbTransaction, out IDbContextTransaction? newDbContextTransaction, TransactionUsage.Reuse, null);

		return new(context, newDbContextTransaction);
	}

	public static TContext CreateNewDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		out IDbContextTransaction newDbContextTransaction,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(externalDbConnection, connectionString);
		}

		return SetDbTransaction(dbContext, (IDbContextTransaction)null!, out newDbContextTransaction!, TransactionUsage.CreateNew, transactionIsolationLevel);
	}

	public static TContext CreateNewDbContextWithoutTransaction<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(externalDbConnection, connectionString);
		}

		return dbContext;
	}

	public static TContext SetDbTransaction<TContext, TIdentity>(
		TContext dbContext,
		IDbContextTransaction? existingDbContextTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel)
		where TContext : DbContext
		where TIdentity : struct
	{
		newDbContextTransaction = null;

		if (dbContext == null)
			throw new ArgumentNullException(nameof(dbContext));

		if (transactionUsage == TransactionUsage.NONE)
			return dbContext;

		if (transactionUsage == TransactionUsage.Reuse)
		{
			if (existingDbContextTransaction == null)
			{
				throw new ArgumentNullException(nameof(existingDbContextTransaction));

				//if (transactionIsolationLevel.HasValue)
				//{
				//	newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
				//}
				//else
				//{
				//	newDbContextTransaction = dbContext.Database.BeginTransaction();
				//}

				//return dbContext;
			}
			else
			{
				if (dbContext.Database.CurrentTransaction == null)
				{
					newDbContextTransaction = existingDbContextTransaction;
					dbContext.Database.UseTransaction(newDbContextTransaction.GetDbTransaction());
					return dbContext;
				}
				else
				{
					if (dbContext.Database.CurrentTransaction.TransactionId != existingDbContextTransaction.TransactionId)
						throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");

					return dbContext;
				}
			}
		}

		if (transactionUsage == TransactionUsage.CreateNew)
		{
			if (dbContext.Database.CurrentTransaction == null)
			{
				if (transactionIsolationLevel.HasValue)
				{
					newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
				}
				else
				{
					newDbContextTransaction = dbContext.Database.BeginTransaction();
				}

				return dbContext;
			}
			else
			{
				throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");
			}
		}

		return dbContext;
	}

	public static TContext SetDbTransaction<TContext, TIdentity>(
		TContext dbContext,
		DbTransaction? existingTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel)
		where TContext : DbContext
		where TIdentity : struct
	{
		newDbContextTransaction = null;

		if (dbContext == null)
			throw new ArgumentNullException(nameof(dbContext));

		if (transactionUsage == TransactionUsage.NONE)
			return dbContext;

		if (transactionUsage == TransactionUsage.Reuse)
		{
			if (existingTransaction == null)
			{
				throw new ArgumentNullException(nameof(existingTransaction));

				//if (transactionIsolationLevel.HasValue)
				//{
				//	newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
				//}
				//else
				//{
				//	newDbContextTransaction = dbContext.Database.BeginTransaction();
				//}

				//return dbContext;
			}
			else
			{
				if (dbContext.Database.CurrentTransaction == null)
				{
					dbContext.Database.UseTransaction(existingTransaction);
					newDbContextTransaction = dbContext.Database.CurrentTransaction;
					return dbContext;
				}
				else
				{
					if (dbContext.Database.CurrentTransaction.GetDbTransaction() != existingTransaction)
						throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");

					return dbContext;
				}
			}
		}

		if (transactionUsage == TransactionUsage.CreateNew)
		{
			if (dbContext.Database.CurrentTransaction == null)
			{
				if (transactionIsolationLevel.HasValue)
				{
					newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
				}
				else
				{
					newDbContextTransaction = dbContext.Database.BeginTransaction();
				}

				return dbContext;
			}
			else
			{
				throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");
			}
		}

		return dbContext;
	}

	public static TContext CreateNewIDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbContextTransaction existingDbContextTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (existingDbContextTransaction == null)
			throw new ArgumentNullException(nameof(existingDbContextTransaction));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(
				existingDbContextTransaction.GetDbTransaction().Connection,
				null);
		}

		dbContext.SetDbTransaction(existingDbContextTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);

		return dbContext;
	}

	public static TContext CreateNewIDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		DbTransaction existingTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (existingTransaction == null)
			throw new ArgumentNullException(nameof(existingTransaction));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(existingTransaction.Connection, null);
		}

		dbContext.SetDbTransaction(existingTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);

		return dbContext;
	}

	public static TContext CreateNewIDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbTransactionFactory dbTransactionFactory,
		out IDbContextTransaction? newDbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (dbTransactionFactory == null)
			throw new ArgumentNullException(nameof(dbTransactionFactory));

		dbTransactionFactory.Initialize();

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(dbTransactionFactory.DbConnection, null);
		}

		var dbTransaction = dbTransactionFactory.GetOrBeginTransaction();
		dbContext.SetDbTransaction(dbTransaction, out newDbContextTransaction, TransactionUsage.Reuse, null);

		return dbContext;
	}

	public static async Task<(TContext dbContext, IDbContextTransaction? newDbContextTransaction)> CreateNewIDbContextAsync<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		IDbTransactionFactory dbTransactionFactory,
		string? commandQueryName = null,
		Guid? idCommandQuery = null,
		CancellationToken cancellationToken = default)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (dbTransactionFactory == null)
			throw new ArgumentNullException(nameof(dbTransactionFactory));

		await dbTransactionFactory.InitializeAsync();

		var context = serviceProvider.GetRequiredService<TContext>();
		if (context is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(dbTransactionFactory.DbConnection, null);
		}

		var dbTransaction = await dbTransactionFactory.GetOrBeginTransactionAsync(cancellationToken);
		context.SetDbTransaction(dbTransaction, out IDbContextTransaction? newDbContextTransaction, TransactionUsage.Reuse, null);

		return new(context, newDbContextTransaction);
	}

	public static TContext CreateNewIDbContext<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		out IDbContextTransaction newDbContextTransaction,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(externalDbConnection, connectionString);
		}

		dbContext.SetDbTransaction((IDbContextTransaction)null!, out newDbContextTransaction!, TransactionUsage.CreateNew, transactionIsolationLevel);

		return dbContext;
	}

	public static TContext CreateNewIDbContextWithoutTransaction<TContext, TIdentity>(
		IServiceProvider serviceProvider,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		where TIdentity : struct
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		var dbContext = serviceProvider.GetRequiredService<TContext>();
		if (dbContext is DbContextBase<TIdentity> dbContextBase)
		{
			dbContextBase.CommandQueryName = commandQueryName;
			dbContextBase.IdCommandQuery = idCommandQuery;
			dbContextBase.SetExternalConnection(externalDbConnection, connectionString);
		}

		return dbContext;
	}
}
