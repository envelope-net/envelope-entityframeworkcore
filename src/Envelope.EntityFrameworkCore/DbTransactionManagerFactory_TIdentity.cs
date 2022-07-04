using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public class TransactionDbContextFactory<TIdentity> : ITransactionManagerFactory
	where TIdentity : struct
{
	private readonly IServiceProvider _serviceProvider;

	public TransactionDbContextFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ITransactionManager Create()
		=> new TransactionDbContext<TIdentity>(_serviceProvider, null, null);

	public ITransactionManager Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionDbContext<TIdentity>(_serviceProvider, configureBehavior, configure);
}
