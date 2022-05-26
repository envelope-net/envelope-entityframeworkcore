using Microsoft.EntityFrameworkCore;
using Envelope.Model.Audit;

namespace Envelope.EntityFrameworkCore;

public interface IAuditableDbContext<TAuditEntry, TIdentity> : IDbContext, IDisposable, IAsyncDisposable
	where TAuditEntry : class, IAuditEntry<TIdentity>, new()
	where TIdentity : struct
{
	DbSet<TAuditEntry> AuditEntry { get; }
}
