using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Envelope.Transactions;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using Envelope.Database.PostgreSql;

namespace Envelope.EntityFrameworkCore;

public class DbContextCache<TIdentity> : IDbContextCache
	where TIdentity : struct
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
		=> DbContextFactory.CreateNewDbContextWithoutTransaction<TContext, TIdentity>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext CreateNewDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		var dbContext =
			DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
				_serviceProvider,
				out newDbContextTransaction,
				transactionIsolationLevel,
				externalDbConnection,
				connectionString,
				commandQueryName,
				idCommandQuery);

		if (transactionManager != null)
		{
			var transaction = newDbContextTransaction;
			var manager = new DbContextTransactionBehaviorObserver(transaction);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return dbContext;
	}

	/// <inheritdoc />
	public TContext CreateNewDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		var result = DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
			_serviceProvider,
			dbContextTransaction,
			out var newDbContextTransaction,
			commandQueryName,
			idCommandQuery);

		if (transactionManager != null && newDbContextTransaction != null)
		{
			var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return result;
	}

	/// <inheritdoc />
	public TContext CreateNewDbContextWithExistingTransaction<TContext>(
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		var result = DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
			_serviceProvider,
			dbTransaction,
			out var newDbContextTransaction,
			commandQueryName,
			idCommandQuery);

		if (transactionManager != null && newDbContextTransaction != null)
		{
			var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return result;
	}

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
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithNewTransaction<TContext>(
			typeof(TContext).FullName!,
			transactionManager,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbContextTransaction,
			transactionManager,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
		=> GetOrCreateDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbTransaction,
			transactionManager,
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
	=> DbContextFactory.CreateNewDbContextWithoutTransaction<TContext, TIdentity>(
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
		ITransactionManager? transactionManager = null,
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
					DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
						_serviceProvider,
						out var newDbContextTransaction,
						transactionIsolationLevel,
						externalDbConnection,
						connectionString,
						commandQueryName,
						idCommandQuery);

				if (transactionManager != null)
				{
					var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
					transactionManager.ConnectTransactionObserver(manager);
				}

				return dbContext;
			}));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbContextTransaction == null)
			throw new ArgumentNullException(nameof(dbContextTransaction));

		var result = _dbContextCache.GetOrAdd(key, (dbContextType)
			=>
			{
				var dbContext = DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
						_serviceProvider,
						dbContextTransaction,
						out var newDbContextTransaction,
						commandQueryName,
						idCommandQuery);

				if (transactionManager != null && newDbContextTransaction != null)
				{
					var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
					transactionManager.ConnectTransactionObserver(manager);
				}

				return dbContext;
			});

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateDbContextWithExistingTransaction<TContext>(
		string key,
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : DbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbTransaction == null)
			throw new ArgumentNullException(nameof(dbTransaction));

		var result = _dbContextCache.GetOrAdd(key, (dbContextType)
			=>
		{
			var dbContext = DbContextFactory.CreateNewDbContext<TContext, TIdentity>(
					_serviceProvider,
					dbTransaction,
					out var newDbContextTransaction,
					commandQueryName,
					idCommandQuery);

			if (transactionManager != null && newDbContextTransaction != null)
			{
				var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
				transactionManager.ConnectTransactionObserver(manager);
			}

			return dbContext;
		});

		return (TContext)result;
	}

	public IDbContextTransaction? GetDbContextTransaction<TContext>()
		where TContext : DbContext
		=> GetDbContextTransaction(typeof(TContext).FullName!);

	public IDbContextTransaction? GetDbContextTransaction(string key)
	{
		if (_dbContextCache.TryGetValue(key, out DbContext? dbContext))
			return dbContext.Database.CurrentTransaction;

		return null;
	}

	/// <inheritdoc />
	public TContext CreateNewIDbContext<TContext>(
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> DbContextFactory.CreateNewIDbContextWithoutTransaction<TContext, TIdentity>(
			_serviceProvider,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext CreateNewIDbContextWithNewTransaction<TContext>(
		out IDbContextTransaction newDbContextTransaction,
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		var dbContext =
			DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
				_serviceProvider,
				out newDbContextTransaction,
				transactionIsolationLevel,
				externalDbConnection,
				connectionString,
				commandQueryName,
				idCommandQuery);

		if (transactionManager != null)
		{
			var transaction = newDbContextTransaction;
			var manager = new DbContextTransactionBehaviorObserver(transaction);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return dbContext;
	}

	/// <inheritdoc />
	public TContext CreateNewIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		var result = DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
			_serviceProvider,
			dbContextTransaction,
			out var newDbContextTransaction,
			commandQueryName,
			idCommandQuery);

		if (transactionManager != null && newDbContextTransaction != null)
		{
			var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction!);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return result;
	}

	/// <inheritdoc />
	public TContext CreateNewIDbContextWithExistingTransaction<TContext>(
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		var result = DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
			_serviceProvider,
			dbTransaction,
			out var newDbContextTransaction,
			commandQueryName,
			idCommandQuery);

		if (transactionManager != null && newDbContextTransaction != null)
		{
			var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction!);
			transactionManager.ConnectTransactionObserver(manager);
		}

		return result;
	}

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
		ITransactionManager? transactionManager = null,
		IsolationLevel? transactionIsolationLevel = null,
		DbConnection? externalDbConnection = null,
		string? connectionString = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithNewTransaction<TContext>(
			typeof(TContext).FullName!,
			transactionManager,
			transactionIsolationLevel,
			externalDbConnection,
			connectionString,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbContextTransaction,
			transactionManager,
			commandQueryName,
			idCommandQuery);

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
		=> GetOrCreateIDbContextWithExistingTransaction<TContext>(
			typeof(TContext).FullName!,
			dbTransaction,
			transactionManager,
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
	=> DbContextFactory.CreateNewIDbContextWithoutTransaction<TContext, TIdentity>(
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
		ITransactionManager? transactionManager = null,
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
				DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
					_serviceProvider,
					out var newDbContextTransaction,
					transactionIsolationLevel,
					externalDbConnection,
					connectionString,
					commandQueryName,
					idCommandQuery);

			if (transactionManager != null)
			{
				var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
				transactionManager.ConnectTransactionObserver(manager);
			}

			return dbContext;
		}));

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		string key,
		IDbContextTransaction dbContextTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbContextTransaction == null)
			throw new ArgumentNullException(nameof(dbContextTransaction));

		var result = _idbContextCache.GetOrAdd(
			key,
			dbContextType =>
			{
				var dbContext = DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
					_serviceProvider,
					dbContextTransaction,
					out var newDbContextTransaction,
					commandQueryName,
					idCommandQuery);

				if (transactionManager != null && newDbContextTransaction != null)
				{
					var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
					transactionManager.ConnectTransactionObserver(manager);
				}

				return dbContext;
			});

		return (TContext)result;
	}

	/// <inheritdoc />
	public TContext GetOrCreateIDbContextWithExistingTransaction<TContext>(
		string key,
		DbTransaction dbTransaction,
		ITransactionManager? transactionManager = null,
		string? commandQueryName = null,
		Guid? idCommandQuery = null)
		where TContext : IDbContext
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (dbTransaction == null)
			throw new ArgumentNullException(nameof(dbTransaction));

		var result = _idbContextCache.GetOrAdd(
			key,
			dbContextType =>
			{
				var dbContext = DbContextFactory.CreateNewIDbContext<TContext, TIdentity>(
					_serviceProvider,
					dbTransaction,
					out var newDbContextTransaction,
					commandQueryName,
					idCommandQuery);

				if (transactionManager != null && newDbContextTransaction != null)
				{
					var manager = new DbContextTransactionBehaviorObserver(newDbContextTransaction);
					transactionManager.ConnectTransactionObserver(manager);
				}

				return dbContext;
			});

		return (TContext)result;
	}

	public IDbContextTransaction? GetIDbContextTransaction<TContext>()
		where TContext : IDbContext
		=> GetIDbContextTransaction(typeof(TContext).FullName!);

	public IDbContextTransaction? GetIDbContextTransaction(string key)
	{
		if (_dbContextCache.TryGetValue(key, out DbContext? dbContext))
			return dbContext.Database.CurrentTransaction;

		return null;
	}
}
