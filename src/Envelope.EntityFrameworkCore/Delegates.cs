using System.Data.Common;

namespace Envelope.EntityFrameworkCore;

public delegate ValueTask<DbTransaction> TransactionFactoryAsync(CancellationToken cancellationToken = default);
