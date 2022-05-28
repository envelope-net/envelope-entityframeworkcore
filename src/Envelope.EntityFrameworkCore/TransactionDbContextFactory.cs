using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public class TransactionDbContextFactory : ITransactionContextFactory
{
	private readonly IServiceProvider _serviceProvider;

	public TransactionDbContextFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ITransactionContext Create()
		=> new TransactionDbContext(_serviceProvider, null, null);

	public ITransactionContext Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionDbContext(_serviceProvider, configureBehavior, configure);
}
