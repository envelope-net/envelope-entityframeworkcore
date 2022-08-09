using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryOptions<TContext>
	where TContext : IDbContext
{
	private TContext? _context;
	public ContextFactory<TContext> ContextFactory { get; }
	public DbConnection? ExternalDbConnection { get; }
	public string? ConnectionString { get; }
	public IDbContextTransaction? DbContextTransaction { get; }
	public ITransactionManager? TransactionManager { get; }

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
			context.TransactionManager);
	}

	private QueryOptions(
		ContextFactory<TContext> contextFactory,
		DbConnection? externalDbConnection,
		string? connectionString,
		IDbContextTransaction? dbContextTransaction,
		ITransactionManager? transactionManager)
	{
		ContextFactory = contextFactory;
		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
		DbContextTransaction = dbContextTransaction;
		TransactionManager = transactionManager;
	}

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
	
	public QueryOptions(TContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
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

	public QueryOptions(IServiceProvider serviceProvider, ITransactionManager transactionManager)
		: this(serviceProvider)
	{
		TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
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

	public QueryOptions(ContextFactory<TContext> contextFactory, ITransactionManager transactionManager)
		: this(contextFactory)
	{
		TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
	}

	public Task<TContext> GetContextAsync(CancellationToken cancellationToken = default)
	{
		if (_context != null)
		{
			return Task.FromResult(_context);
		}
		else if (TransactionManager != null)
		{
			return ContextFactory.GetOrCreateDbContextWithNewTransactionAsync(TransactionManager, cancellationToken);
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
}
