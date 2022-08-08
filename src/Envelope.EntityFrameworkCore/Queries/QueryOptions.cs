using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryOptions<TContext>
	where TContext : IDbContext
{
	public QueryContextFactory<TContext> QueryContextFactory { get; }
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
			QueryContextFactory<TContext>.Create(context.QueryContextFactory),
			context.ExternalDbConnection,
			context.ConnectionString,
			context.DbContextTransaction,
			context.TransactionManager);
	}

	private QueryOptions(
		QueryContextFactory<TContext> queryContextFactory,
		DbConnection? externalDbConnection,
		string? connectionString,
		IDbContextTransaction? dbContextTransaction,
		ITransactionManager? transactionManager)
	{
		QueryContextFactory = queryContextFactory;
		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
		DbContextTransaction = dbContextTransaction;
		TransactionManager = transactionManager;
	}

	public QueryOptions(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		QueryContextFactory = serviceProvider.GetRequiredService<QueryContextFactory<TContext>>();
	}

	public QueryOptions(QueryContextFactory<TContext> queryContextFactory)
	{
		QueryContextFactory = queryContextFactory ?? throw new ArgumentNullException(nameof(queryContextFactory));
	}

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

	public QueryOptions(QueryContextFactory<TContext> queryContextFactory, DbConnection? externalDbConnection, string? connectionString)
		: this(queryContextFactory)
	{
		if (externalDbConnection == null && string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException($"{nameof(externalDbConnection)} == null AND {nameof(connectionString)} == null");

		ExternalDbConnection = externalDbConnection;
		ConnectionString = connectionString;
	}

	public QueryOptions(QueryContextFactory<TContext> queryContextFactory, IDbContextTransaction dbContextTransaction)
		: this(queryContextFactory)
	{
		DbContextTransaction = dbContextTransaction ?? throw new ArgumentNullException(nameof(dbContextTransaction));
	}

	public QueryOptions(QueryContextFactory<TContext> queryContextFactory, ITransactionManager transactionManager)
		: this(queryContextFactory)
	{
		TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
	}

	public Task<TContext> GetContextAsync(CancellationToken cancellationToken = default)
	{
		if (TransactionManager != null)
		{
			return QueryContextFactory.GetOrCreateDbContextWithNewTransactionAsync(TransactionManager, cancellationToken);
		}
		else if (DbContextTransaction != null)
		{
			return Task.FromResult(QueryContextFactory.GetOrCreateDbContextWithExistingTransaction(DbContextTransaction));
		}
		else if (ExternalDbConnection != null || !string.IsNullOrWhiteSpace(ConnectionString))
		{
			return Task.FromResult(QueryContextFactory.GetOrCreateDbContextWithoutTransaction(ExternalDbConnection, ConnectionString));
		}
		else
		{
			return QueryContextFactory.GetOrCreateDbContextWithNewTransactionAsync(cancellationToken);
		}
	}
}
