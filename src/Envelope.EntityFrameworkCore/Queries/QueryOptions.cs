using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryOptions<TContext> : IDisposable, IAsyncDisposable
	where TContext : IDbContext
{
	private bool _disposed;

	public TContext? Context { get; }
	public ContextFactory<TContext> ContextFactory { get; }
	public DbConnection? ExternalDbConnection { get; }
	public string? ConnectionString { get; }
	public IDbContextTransaction? DbContextTransaction { get; }
	public ITransactionCoordinator? TransactionCoordinator { get; }

	public static QueryOptions<TContext> Create<TOtherContext>(QueryOptions<TOtherContext> context)
		where TOtherContext : IDbContext
	{
		if (context == null)
			throw new ArgumentNullException(nameof(context));

		return new QueryOptions<TContext>(
			ContextFactory<TContext>.Create(context.ContextFactory),
			context.ExternalDbConnection,
			context.ConnectionString,
			context.DbContextTransaction,
			context.TransactionCoordinator);
	}

	private QueryOptions(
		ContextFactory<TContext> contextFactory,
		DbConnection? externalDbConnection,
		string? connectionString,
		IDbContextTransaction? dbContextTransaction,
		ITransactionCoordinator? transactionCoordinator)
	{
		ContextFactory = contextFactory;
		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
		DbContextTransaction = dbContextTransaction;
		TransactionCoordinator = transactionCoordinator;
	}

	public ITransactionCoordinator? GetTransactionCoordinator()
		=> TransactionCoordinator ?? ContextFactory?.GetTransactionCoordinator();

	public QueryOptions(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		ContextFactory = serviceProvider.GetRequiredService<ContextFactory<TContext>>();
	}

	public QueryOptions(ContextFactory<TContext> contextFactory)
	{
		ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	
	internal QueryOptions(TContext context)
	{
		Context = context ?? throw new ArgumentNullException(nameof(context));
	}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public QueryOptions(IServiceProvider serviceProvider, DbConnection? externalDbConnection, string? connectionString)
		: this(serviceProvider)
	{
		if (externalDbConnection == null && string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException($"{nameof(externalDbConnection)} == null AND {nameof(connectionString)} == null");

		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
	}

	public QueryOptions(IServiceProvider serviceProvider, IDbContextTransaction dbContextTransaction)
		: this(serviceProvider)
	{
		DbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	public QueryOptions(IServiceProvider serviceProvider, ITransactionCoordinator transactionCoordinator)
		: this(serviceProvider)
	{
		TransactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	}

	public QueryOptions(ContextFactory<TContext> contextFactory, DbConnection? externalDbConnection, string? connectionString)
		: this(contextFactory)
	{
		if (externalDbConnection == null && string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException($"{nameof(externalDbConnection)} == null AND {nameof(connectionString)} == null");

		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
	}

	public QueryOptions(ContextFactory<TContext> contextFactory, IDbContextTransaction dbContextTransaction)
		: this(contextFactory)
	{
		DbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	public QueryOptions(ContextFactory<TContext> contextFactory, ITransactionCoordinator transactionCoordinator)
		: this(contextFactory)
	{
		TransactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
	}

	public Task<TContext> GetContextAsync(CancellationToken cancellationToken = default)
	{
		if (Context != null)
		{
			return Task.FromResult(Context);
		}
		else if (TransactionCoordinator != null)
		{
			return ContextFactory.GetOrCreateDbContextWithNewTransactionAsync(TransactionCoordinator, cancellationToken);
		}
		else if (DbContextTransaction != null)
		{
			return Task.FromResult(ContextFactory.GetOrCreateDbContextWithExistingTransaction(DbContextTransaction));
		}
		else if (ExternalDbConnection != null || !string.IsNullOrWhiteSpace(ConnectionString))
		{
			return Task.FromResult(ContextFactory.GetOrCreateDbContextWithoutTransaction(ExternalDbConnection, ConnectionString));
		}
		else
		{
			return ContextFactory.GetOrCreateDbContextWithNewTransactionAsync(cancellationToken);
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
		if (ContextFactory != null)
			await ContextFactory.DisposeAsync();

		if (TransactionCoordinator != null)
			await TransactionCoordinator.DisposeAsync();

		try
		{
			if (DbContextTransaction != null)
				await DbContextTransaction.DisposeAsync();
		}
		catch { }

		try
		{
			if (ExternalDbConnection != null)
				await ExternalDbConnection.DisposeAsync();
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
			if (ContextFactory != null)
				ContextFactory.Dispose();

			if (TransactionCoordinator != null)
				TransactionCoordinator.Dispose();

			try
			{
				if (DbContextTransaction != null)
					DbContextTransaction.Dispose();
			}
			catch { }

			try
			{
				if (ExternalDbConnection != null)
					ExternalDbConnection.Dispose();
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
