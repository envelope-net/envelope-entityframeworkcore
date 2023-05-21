using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Envelope.Transactions;
using Envelope.Database.PostgreSql;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
	private static readonly string _dbContextCacheType = typeof(IDbContextCache).FullName!;

	public static IServiceCollection AddDbContextCacheFactory(this IServiceCollection services, IDbContextTransactionBehaviorObserverFactory dbContextTransactionBehaviorObserverFactory)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));
		if (dbContextTransactionBehaviorObserverFactory == null)
			throw new ArgumentNullException(nameof(dbContextTransactionBehaviorObserverFactory));

		services.TryAddTransient<ITransactionCoordinator, TransactionCoordinator>();
		services.AddTransient<ITransactionCacheFactoryStore>(sp => new TransactionCacheFactoryStore(
			_dbContextCacheType,
			serviceProvider => new DbContextCache(serviceProvider, dbContextTransactionBehaviorObserverFactory)));
		return services;
	}
}
