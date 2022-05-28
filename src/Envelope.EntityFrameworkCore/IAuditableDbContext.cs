using Microsoft.EntityFrameworkCore;
using Envelope.Model.Audit;

namespace Envelope.EntityFrameworkCore;

public interface IAuditableDbContext<TAuditEntry> : IDbContext, IDisposable, IAsyncDisposable
	where TAuditEntry : class, IAuditEntry, new()
{
	DbSet<TAuditEntry> AuditEntry { get; }
}
