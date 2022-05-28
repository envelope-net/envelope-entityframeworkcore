using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDbContextCache(this IServiceCollection services, ServiceLifetime dbContextCacheLifetime = ServiceLifetime.Scoped)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.TryAddTransient<ITransactionContextFactory, TransactionDbContextFactory>();
		services.TryAdd(new ServiceDescriptor(typeof(IDbContextCache), typeof(DbContextCache), dbContextCacheLifetime));
		return services;
	}

	public static IServiceCollection AddDbContextCache<TIdentity>(this IServiceCollection services, ServiceLifetime dbContextCacheLifetime = ServiceLifetime.Scoped)
		where TIdentity : struct
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.TryAddTransient<ITransactionContextFactory, TransactionDbContextFactory<TIdentity>>();
		services.TryAdd(new ServiceDescriptor(typeof(IDbContextCache), typeof(DbContextCache<TIdentity>), dbContextCacheLifetime));
		return services;
	}
}
