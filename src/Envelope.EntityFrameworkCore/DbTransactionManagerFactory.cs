using Envelope.EntityFrameworkCore.Internal;
using Envelope.Transactions;

namespace Envelope.EntityFrameworkCore;

public class DbTransactionManagerFactory : ITransactionManagerFactory
{
	private readonly IServiceProvider _serviceProvider;

	public DbTransactionManagerFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ITransactionManager Create()
		=> new DbTransactionManager(_serviceProvider, null, null);

	public ITransactionManager Create(Action<ITransactionBehaviorObserverConnector>? configureBehavior, Action<ITransactionObserverConnector>? configure)
		=> new DbTransactionManager(_serviceProvider, configureBehavior, configure);
}
