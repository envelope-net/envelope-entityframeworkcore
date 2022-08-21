using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
	private static readonly string _dbContextCacheType = typeof(IDbContextCache).FullName!;

	public static IServiceCollection AddDbContextCacheFactory(this IServiceCollection services)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.TryAddTransient<ITransactionCoordinator, TransactionCoordinator>();
		services.AddTransient<ITransactionCacheFactoryStore>(sp => new TransactionCacheFactoryStore(
			_dbContextCacheType,
			serviceProvider => new DbContextCache(serviceProvider)));
		return services;
	}
}
