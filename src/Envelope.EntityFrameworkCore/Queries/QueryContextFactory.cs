using Envelope.Threading;
using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryContextFactory<TContext>
	where TContext : IDbContext
{
	private readonly AsyncLock _locker = new();

	private DbTransaction? _dbTransaction;

	private readonly IServiceProvider _serviceProvider;
	private readonly TransactionFactoryAsync _transactionFactory;
	private readonly IDbContextProvider _dbContextProvider;
	private readonly Lazy<ITransactionManager> _transactionManager;

	public static QueryContextFactory<TContext> Create<TOtherContext>(QueryContextFactory<TOtherContext> factory)
		where TOtherContext : IDbContext
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory));

		return new QueryContextFactory<TContext>(
			factory._serviceProvider,
			factory._transactionFactory,
			factory._dbContextProvider,
			factory._transactionManager);
	}

	private QueryContextFactory(
		IServiceProvider serviceProvider,
		TransactionFactoryAsync transactionFactory,
		IDbContextProvider dbContextProvider,
		Lazy<ITransactionManager> transactionManager)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
		_dbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
		_transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
	}

	public QueryContextFactory(
		IServiceProvider serviceProvider,
		TransactionFactoryAsync transactionFactory,
		IDbContextProvider dbContextProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
		_dbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));

		_transactionManager = new(() => _serviceProvider.GetService<ITransactionManagerFactory>()?.Create() ?? TransactionManagerFactory.CreateTransactionManager());
	}

	//public TContext GetOrCreateDbContextWithoutTransaction(QueryOptions queryOptions)
	//	=> queryOptions != null
	//		? ((queryOptions.ExternalDbConnection != null || !string.IsNullOrWhiteSpace(queryOptions.ConnectionString))
	//			? GetOrCreateDbContextWithoutTransaction(queryOptions.ExternalDbConnection, queryOptions.ConnectionString)
	//			: throw new InvalidOperationException($"{nameof(GetOrCreateDbContextWithoutTransaction)}: invalid {nameof(queryOptions)}"))
	//		: throw new ArgumentNullException(nameof(queryOptions));

	public TContext GetOrCreateDbContextWithoutTransaction(DbConnection? externalDbConnection = null, string? connectionString = null)
		=> _dbContextProvider.GetOrCreateDbContextWithoutTransaction<TContext>(externalDbConnection, connectionString);

	//public TContext GetOrCreateDbContextWithExistingTransaction(QueryOptions queryOptions)
	//	=> queryOptions != null
	//		? (queryOptions.DbContextTransaction != null
	//			? GetOrCreateDbContextWithExistingTransaction(queryOptions.DbContextTransaction)
	//			: throw new InvalidOperationException($"{nameof(GetOrCreateDbContextWithExistingTransaction)}: invalid {nameof(queryOptions)}"))
	//		: throw new ArgumentNullException(nameof(queryOptions));

	public TContext GetOrCreateDbContextWithExistingTransaction(IDbContextTransaction dbContextTransaction)
		=> _dbContextProvider.GetOrCreateDbContextWithExistingTransaction<TContext>(dbContextTransaction, null);

	//public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(
	//	QueryOptions queryOptions,
	//	CancellationToken cancellationToken = default)
	//	=> queryOptions != null
	//		? (queryOptions.TransactionManager != null
	//			? GetOrCreateDbContextWithNewTransactionAsync(queryOptions.TransactionManager, cancellationToken)
	//			: throw new InvalidOperationException($"{nameof(GetOrCreateDbContextWithNewTransactionAsync)}: invalid {nameof(queryOptions)}"))
	//		: throw new ArgumentNullException(nameof(queryOptions));

	public Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(CancellationToken cancellationToken = default)
		=> GetOrCreateDbContextWithNewTransactionAsync(_transactionManager.Value, cancellationToken);

	public async Task<TContext> GetOrCreateDbContextWithNewTransactionAsync(
		ITransactionManager transactionManager,
		CancellationToken cancellationToken = default)
	{
		using (await _locker.LockAsync())
		{
			if (_dbTransaction == null)
				_dbTransaction = await _transactionFactory(cancellationToken);
		}

		var context = _dbContextProvider.GetOrCreateDbContextWithExistingTransaction<TContext>(_dbTransaction, transactionManager);
		return context;
	}
}
