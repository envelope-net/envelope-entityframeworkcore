﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Envelope.Model;
using Envelope.Model.Audit;

namespace Envelope.EntityFrameworkCore.Internal;

internal class AuditEntryInternal
{
	public EntityEntry Entry { get; }

	public DateTime CreatedUtc { get; set; }
	public Guid? IdUser { get; set; }
	public string TableName { get; }
	public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();
	public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
	public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
	public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
	public DbOperation DbOperation { get; set; }
	public List<string> ChangedColumns { get; } = new List<string>();
	public bool HasTemporaryProperties => TemporaryProperties.Any();
	public string? CommandQueryName { get; set; }
	public Guid? IdCommandQuery { get; set; }
	public Guid? CorrelationId { get; set; }

	public AuditEntryInternal(EntityEntry entry)
	{
		Entry = entry;
		TableName = entry.Entity.GetType().Name;
	}

	public TAuditEntry ToAudit<TAuditEntry>(Guid auditCorrelationId)
		where TAuditEntry : class, IAuditEntry, new()
		=> new()
		{
			IdUser = IdUser,
			IdAuditType = (int)DbOperation,
			CreatedUtc = CreatedUtc,
			TableName = TableName,
			PrimaryKey = System.Text.Json.JsonSerializer.Serialize(KeyValues),
			OldValues = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues),
			NewValues = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(NewValues),
			AffectedColumns = ChangedColumns.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(ChangedColumns),
			AuditCorrelationId = auditCorrelationId,
			CommandQueryName = CommandQueryName,
			IdCommandQuery = IdCommandQuery,
			CorrelationId = CorrelationId
		};
}
