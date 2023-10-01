using Envelope.Exceptions;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryOptions<TContext> : IDisposable, IAsyncDisposable
	where TContext : IDbContext
{
	private bool _disposed;

	private TContext? _context;
	private string? _connectionId;
	private ContextFactory<TContext> _contextFactory;
	private DbConnection? _externalDbConnection;
	private string? _connectionString;
	private IDbContextTransaction? _dbontextTransaction;
	private ITransactionCoordinator? _transactionCoordinator;

	public TContext? Context => _context;

	public static QueryOptions<TContext> Create<TOtherContext>(QueryOptions<TOtherContext> queryOptions)
		where TOtherContext : IDbContext
	{
		Throw.ArgumentNull(queryOptions);

		return new QueryOptions<TContext>(
			ContextFactory<TContext>.Create(queryOptions._contextFactory),
			queryOptions._externalDbConnection,
			queryOptions._connectionString,
			queryOptions._dbontextTransaction,
			queryOptions._transactionCoordinator);
	}

	private QueryOptions(
		ContextFactory<TContext> contextFactory,
		DbConnection? externalDbConnection,
		string? connectionString,
		IDbContextTransaction? dbContextTransaction,
		ITransactionCoordinator? transactionCoordinator)
	{
		_contextFactory = contextFactory;
		_externalDbConnection = externalDbConnection;
		_connectionString = connectionString;
		_dbontextTransaction = dbContextTransaction;
		_transactionCoordinator = transactionCoordinator;
	}

	public QueryOptions(ContextFactory<TContext> contextFactory)
	{
		Throw.ArgumentNull(contextFactory);
		_contextFactory = contextFactory;
		_transactionCoordinator = _contextFactory.GetTransactionCoordinator();
	}

	public QueryOptions(IServiceProvider serviceProvider)
		: this(serviceProvider?.GetRequiredService<ContextFactory<TContext>>()!)
	{
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	
	internal QueryOptions(TContext context, ITransactionCoordinator transactionCoordinator)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	//zatial nikdy nepouzivane
	//public QueryOptions(IServiceProvider serviceProvider, DbConnection? externalDbConnection, string? connectionString)
	//	: this(serviceProvider)
	//{
	//	if (externalDbConnection == null && string.IsNullOrWhiteSpace(connectionString))
	//		throw new InvalidOperationException($"{nameof(externalDbConnection)} == null AND {nameof(connectionString)} == null");

	//	_externalDbConnection = externalDbConnection;
	//	_connectionString = connectionString;
	//}

	//zatial nikdy nepouzivane
	//public QueryOptions(IServiceProvider serviceProvider, IDbContextTransaction dbContextTransaction)
	//	: this(serviceProvider)
	//{
	//	_dbontextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	//}

	//zatial nikdy nepouzivane
	//public QueryOptions(IServiceProvider serviceProvider, ITransactionCoordinator transactionCoordinator)
	//	: this(serviceProvider)
	//{
	//	_transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	//}

	public QueryOptions(ContextFactory<TContext> contextFactory, DbConnection? externalDbConnection, string? connectionString)
		: this(contextFactory)
	{
		if (externalDbConnection == null && string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException($"{nameof(externalDbConnection)} == null AND {nameof(connectionString)} == null");

		_externalDbConnection = externalDbConnection;
		_connectionString = connectionString;
	}

	public QueryOptions(ContextFactory<TContext> contextFactory, IDbContextTransaction dbContextTransaction)
		: this(contextFactory)
	{
		_dbontextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	//zatial nikdy nepouzivane
	//public QueryOptions(ContextFactory<TContext> contextFactory, ITransactionCoordinator transactionCoordinator)
	//	: this(contextFactory)
	//{
	//	_transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	//}

	public ITransactionCoordinator? GetTransactionCoordinator()
		=> _transactionCoordinator ??= _contextFactory?.GetTransactionCoordinator();

	public async Task<TContext> GetContextAsync(
		string connectionId,
		CancellationToken cancellationToken = default)
	{
		if (_context != null)
		{
			return _context;
		}
		else if (_transactionCoordinator != null)
		{
			if (string.IsNullOrWhiteSpace(connectionId))
				connectionId = _connectionId!;

			_context = await _contextFactory.GetOrCreateDbContextWithNewTransactionAsync(connectionId, _transactionCoordinator, cancellationToken);
			return _context;
		}
		else if (_dbontextTransaction != null)
		{
			_context = _contextFactory.GetOrCreateDbContextWithExistingTransaction(connectionId, _dbontextTransaction);
			return _context;
		}
		else if (_externalDbConnection != null || !string.IsNullOrWhiteSpace(_connectionString))
		{
			_context = _contextFactory.GetOrCreateDbContextWithoutTransaction(connectionId, _externalDbConnection, _connectionString);
			return _context;
		}
		else
		{
			if (string.IsNullOrWhiteSpace(connectionId))
				connectionId = _connectionId!;

			_context = await _contextFactory.GetOrCreateDbContextWithNewTransactionAsync(connectionId, cancellationToken);
			return _context;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual async ValueTask DisposeAsyncCoreAsync()
	{
		if (_contextFactory != null)
			await _contextFactory.DisposeAsync();

		if (_transactionCoordinator != null)
			await _transactionCoordinator.DisposeAsync();

		try
		{
			if (_dbontextTransaction != null)
				await _dbontextTransaction.DisposeAsync();
		}
		catch { }

		try
		{
			if (_externalDbConnection != null)
				await _externalDbConnection.DisposeAsync();
		}
		catch { }
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			if (_contextFactory != null)
				_contextFactory.Dispose();

			if (_transactionCoordinator != null)
				_transactionCoordinator.Dispose();

			try
			{
				if (_dbontextTransaction != null)
					_dbontextTransaction.Dispose();
			}
			catch { }

			try
			{
				if (_externalDbConnection != null)
					_externalDbConnection.Dispose();
			}
			catch { }
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
