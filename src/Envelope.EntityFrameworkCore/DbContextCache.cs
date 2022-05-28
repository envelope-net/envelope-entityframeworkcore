﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Envelope.Transactions;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using Envelope.Database.PostgreSql;

namespace Envelope.EntityFrameworkCore;

public class DbContextCache : IDbContextCache
{
	private readonly ConcurrentDictionary<string, DbContext> _dbContextCache;
	private readonly ConcurrentDictionary<string, IDbContext> _idbContextCache;
	private readonly IServiceProvider _serviceProvider;

	public DbContextCache(IServiceProvider serviceProvider)
	{
		_dbContextCache = new ConcurrentDictionary<string, DbContext>();
		_idbContextCache = new ConcurrentDictionary<string, IDbContext>();
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	/// <inheritdoc />
	public TContext CreateNewDbContext<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> DbContextFactory.CreateNewDbContextWithoutTransaction<TContext>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext CreateNewDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		var dbContext= 
			DbContextFactory.CreateNewDbContext<TContext>(
				_serviceProvider,
				out newDbContextTransaction,
				transactionIsolationLevel,
				externalDbConnection,
				connectionString,
				commandQueryName,
				idCommandQuery);

		if (transactionContext != null)
		{
			var transaction = newDbContextTransaction;
			var manager = new DbContextTransactionBehaviorObserver(transaction);
			transactionContext.ConnectTransactionManager(manager);
		}

		return dbContext;
	}

	/// <inheritdoc />
	public TContext CreateNewDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> DbContextFactory.CreateNewDbContext<TContext>(
			_serviceProvider,
			dbContextTransaction,
			out _,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithoutTransaction<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithoutTransaction<TContext>(
			typeof(TContext).FullName!,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithNewTransaction<TContext>(
			typeof(TContext).FullName!,
			transactionContext,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbContextTransaction,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithoutTransaction<TContext>(
		string key,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		var result = _dbContextCache.GetOrAdd(key, (dbContextType)
	=> DbContextFactory.CreateNewDbContextWithoutTransaction<TContext>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithNewTransaction<TContext>(
		string key,
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		var result = _dbContextCache.GetOrAdd(key, (Func<string, DbContext>)((dbContextType)
			=>
			{
				var dbContext =
					DbContextFactory.CreateNewDbContext<TContext>(
						_serviceProvider,
						out var newDbContextTransaction,
						transactionIsolationLevel,
						externalDbConnection,
						connectionString,
						commandQueryName,
						idCommandQuery);

				if (transactionContext != null)
				{
					var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
					transactionContext.ConnectTransactionManager(manager);
				}

				return dbContext;
		}));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbContextTransaction == null)
			throw new ArgumentNullException(nameof(dbContextTransaction));

		var result = _dbContextCache.GetOrAdd(key, (dbContextType)
			=> DbContextFactory.CreateNewDbContext<TContext>(
				_serviceProvider,
				dbContextTransaction,
				out _,
				commandQueryName,
				idCommandQuery));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext CreateNewIDbContext<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> DbContextFactory.CreateNewIDbContextWithoutTransaction<TContext>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext CreateNewIDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		var dbContext =
			DbContextFactory.CreateNewIDbContext<TContext>(
				_serviceProvider,
				out newDbContextTransaction,
				transactionIsolationLevel,
				externalDbConnection,
				connectionString,
				commandQueryName,
				idCommandQuery);

		if (transactionContext != null)
		{
			var transaction = newDbContextTransaction;
			var manager = new DbContextTransactionBehaviorObserver(transaction);
			transactionContext.ConnectTransactionManager(manager);
		}

		return dbContext;
	}

	/// <inheritdoc />
	public TContext CreateNewIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> DbContextFactory.CreateNewIDbContext<TContext>(
			_serviceProvider,
			dbContextTransaction,
			out _,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithoutTransaction<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithoutTransaction<TContext>(
			typeof(TContext).FullName!,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithNewTransaction<TContext>(
			typeof(TContext).FullName!,
			transactionContext,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbContextTransaction,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithoutTransaction<TContext>(
		string key,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		var result = _idbContextCache.GetOrAdd(key, (dbContextType)
	=> DbContextFactory.CreateNewIDbContextWithoutTransaction<TContext>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithNewTransaction<TContext>(
		string key,
		ITransactionContext? transactionContext = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		var result = _idbContextCache.GetOrAdd(key, (Func<string, IDbContext>)((dbContextType)
			=>
		{
			var dbContext =
				DbContextFactory.CreateNewIDbContext<TContext>(
					_serviceProvider,
					out var newDbContextTransaction,
					transactionIsolationLevel,
					externalDbConnection,
					connectionString,
					commandQueryName,
					idCommandQuery);

			if (transactionContext != null)
			{
				var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
				transactionContext.ConnectTransactionManager(manager);
			}

			return dbContext;
		}));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbContextTransaction == null)
			throw new ArgumentNullException(nameof(dbContextTransaction));

		var result = _idbContextCache.GetOrAdd(key, (dbContextType)
			=> DbContextFactory.CreateNewIDbContext<TContext>(
				_serviceProvider,
				dbContextTransaction,
				out _,
				commandQueryName,
				idCommandQuery));

		return (TContext)result;
	}
}