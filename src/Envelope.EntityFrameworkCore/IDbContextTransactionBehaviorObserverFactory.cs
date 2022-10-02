using Envelope.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Envelope.Database.PostgreSql;

public interface IDbContextTransactionBehaviorObserverFactory
{
	ITransactionBehaviorObserver Create(IDbContextTransaction dbContextTransaction, int waitForConnectionExecutingInMilliseconds = 50, int waitForConnectionExecutingCount = 40);
}
