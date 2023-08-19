using Envelope.Database;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class ContextFactory<TContext> : IDisposable, IAsyncDisposable
	where TContext : IDbContext
{
	private readonly Lazy<ITransactionCoordinator> _transactionCoordinator;

	private bool _disposed;

	public IServiceProvider ServiceProvider { get; }
	public ITransactionCoordinator? TransactionCoordinator => _transactionCoordinator.IsValueCreated
		? _transactionCoordinator.Value
		: null;

	public ITransactionCoordinator GetTransactionCoordinator()
		=> _transactionCoordinator.Value;

	public static ContextFactory<TContext> Create<TOtherContext>(ContextFactory<TOtherContext> factory)
		where TOtherContext : IDbContext
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory));

		return new ContextFactory<TContext>(
			factory.ServiceProvider,
			factory._transactionCoordinator);
	}

	private ContextFactory(
		IServiceProvider serviceProvider,
		Lazy<ITransactionCoordinator> transactionCoordinator)
	{
		ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	}

	public ContextFactory(
		IServiceProvider serviceProvider,
		DbConnectionFactoryAsync dbConnectionFactoryAsync)
	{
		ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		if (dbConnectionFactoryAsync == null)
			throw new ArgumentNullException(nameof(dbConnectionFactoryAsync));

		_transactionCoordinator = new(() => ServiceProvider.GetRequiredService<ITransactionCoordinator>());
	}

	public TContext GetOrCreateDbContextWithoutTransaction(DbConnection? externalDbConnection = null, string? connectionString = null)
		=> _transactionCoordinator.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithoutTransaction<TContext>(externalDbConnection, connectionString, null, null);

	public TContext GetOrCreateDbContextWithExistingTransaction(IDbContextTransaction dbContextTransaction)
		=> _transactionCoordinator.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransaction<TContext>(dbContextTransaction, null, null, null);

	public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(
		string connectionId,
		CancellationToken cancellationToken = default)
		=> _transactionCoordinator.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(_transactionCoordinator.Value.TransactionController.GetTransactionCache<IDbTransactionFactory>(), connectionId, _transactionCoordinator.Value, null, null, cancellationToken);

	public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(
		ITransactionCoordinator transactionCoordinator,
		string connectionId,
		CancellationToken cancellationToken = default)
	{
		if (transactionCoordinator == null)
			throw new ArgumentNullException(nameof(transactionCoordinator));

		var cache = transactionCoordinator.TransactionController.GetTransactionCache<IDbContextCache>();
		var result =
			cache.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(
				transactionCoordinator.TransactionController.GetTransactionCache<IDbTransactionFactory>(),
				connectionId,
				transactionCoordinator,
				null,
				null,
				cancellationToken);

		return result;
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
		if (_transactionCoordinator.IsValueCreated)
			await _transactionCoordinator.Value.DisposeAsync();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			if (_transactionCoordinator.IsValueCreated)
				_transactionCoordinator.Value.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
