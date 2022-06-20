using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public class TransactionDbContextFactory : ITransactionManagerFactory
{
	private readonly IServiceProvider _serviceProvider;

	public TransactionDbContextFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ITransactionManager Create()
		=> new TransactionDbContext(_serviceProvider, null, null);

	public ITransactionManager Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new TransactionDbContext(_serviceProvider, configureBehavior, configure);
}
