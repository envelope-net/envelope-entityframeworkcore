using Envelope.Database;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class ContextFactory<TContext> : IDisposable, IAsyncDisposable
	where TContext : IDbContext
{
	private readonly IServiceProvider _serviceProvider;
	private readonly Lazy<ITransactionCoordinator> _transactionCoordiantor;

	private bool _disposed;

	public static ContextFactory<TContext> Create<TOtherContext>(ContextFactory<TOtherContext> factory)
		where TOtherContext : IDbContext
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory));

		return new ContextFactory<TContext>(
			factory._serviceProvider,
			factory._transactionCoordiantor);
	}

	private ContextFactory(
		IServiceProvider serviceProvider,
		Lazy<ITransactionCoordinator> transactionCoordiantor)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_transactionCoordiantor = transactionCoordiantor ?? throw new ArgumentNullException(nameof(transactionCoordiantor));
	}

	public ContextFactory(
		IServiceProvider serviceProvider,
		DbConnectionFactoryAsync dbConnectionFactoryAsync)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		if (dbConnectionFactoryAsync == null)
			throw new ArgumentNullException(nameof(dbConnectionFactoryAsync));

		_transactionCoordiantor = new(() => _serviceProvider.GetRequiredService<ITransactionCoordinator>());
	}

	public TContext GetOrCreateDbContextWithoutTransaction(DbConnection? externalDbConnection = null, string? connectionString = null)
		=> _transactionCoordiantor.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithoutTransaction<TContext>(externalDbConnection, connectionString, null, null);

	public TContext GetOrCreateDbContextWithExistingTransaction(IDbContextTransaction dbContextTransaction)
		=> _transactionCoordiantor.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransaction<TContext>(dbContextTransaction, null, null, null);

	public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(CancellationToken cancellationToken = default)
		=> _transactionCoordiantor.Value.TransactionController.GetTransactionCache<IDbContextCache>()
			.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(_transactionCoordiantor.Value.TransactionController.GetTransactionCache<IDbTransactionFactory>(), _transactionCoordiantor.Value, null, null, cancellationToken);

	public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(
		ITransactionCoordinator transactionCoordiantor,
		CancellationToken cancellationToken = default)
	{
		if (transactionCoordiantor == null)
			throw new ArgumentNullException(nameof(transactionCoordiantor));

		var cache = transactionCoordiantor.TransactionController.GetTransactionCache<IDbContextCache>();
		var result =
			cache.GetOrCreateIDbContextWithExistingTransactionAsync<TContext>(
				transactionCoordiantor.TransactionController.GetTransactionCache<IDbTransactionFactory>(),
				transactionCoordiantor,
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
		if (_transactionCoordiantor.IsValueCreated)
			await _transactionCoordiantor.Value.DisposeAsync();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			if (_transactionCoordiantor.IsValueCreated)
				_transactionCoordiantor.Value.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
