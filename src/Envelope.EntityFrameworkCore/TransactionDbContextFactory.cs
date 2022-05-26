using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public class TransactionDbContextFactory<TIdentity> : ITransactionContextFactory
	where TIdentity : struct
{
	private readonly IServiceProvider _serviceProvider;

	public TransactionDbContextFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ITransactionContext Create()
		=> new TransactionDbContext<TIdentity>(_serviceProvider, null, null);

	public ITransactionContext Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionDbContext<TIdentity>(_serviceProvider, configureBehavior, configure);
}
