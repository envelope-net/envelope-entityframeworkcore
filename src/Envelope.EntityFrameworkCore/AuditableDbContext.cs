﻿using Envelope.Database;
using Envelope.EntityFrameworkCore.Internal;
using Envelope.Model;
using Envelope.Model.Audit;
using Envelope.Model.Concurrence;
using Envelope.Model.Correlation;
using Envelope.Model.Synchronyzation;
using Envelope.Trace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Envelope.EntityFrameworkCore;

public abstract class AuditableDbContext<TAuditEntry> : DbContextBase, IAuditableDbContext<TAuditEntry>, IDbContext, IDisposable, IAsyncDisposable
	where TAuditEntry : class, IAuditEntry, new()
{
	public DbSet<TAuditEntry> AuditEntry { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public AuditableDbContext(DbContextOptions options, ILogger logger, IApplicationContext appContext/*, disabledEtitiesFromAudit, disabledEtityPropertiesFromAudit*/)
		: base(options, logger, appContext)
	{
	}

	protected AuditableDbContext(ILogger logger, IApplicationContext appContext)
		: base(logger, appContext)
	{
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public override int Save(
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(null, true, null, memberName, sourceFilePath, sourceLineNumber);

	public override int Save(
		ITraceInfo traceInfo,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, true, null, memberName, sourceFilePath, sourceLineNumber);

	public override int Save(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, true, options, memberName, sourceFilePath, sourceLineNumber);

	public override int Save(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, acceptAllChangesOnSuccess, null, memberName, sourceFilePath, sourceLineNumber);

	public override int Save(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		SaveOptions? options,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);

		//if (IsTransactionCommittedDelegate != null)
		//{
		//	var isCommitted = IsTransactionCommittedDelegate();
		//	if (isCommitted)
		//		throw new InvalidOperationException("The underlying transaction has already been committed.");
		//}

		var auditCorrelationId = Guid.NewGuid();
		var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId, options, traceInfo);

		var result = base.SaveWithoutScope(acceptAllChangesOnSuccess);

		if (0 < auditEntriesWithTempProperty.Count)
		{
			OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
			var tmpResult = base.SaveWithoutScope(acceptAllChangesOnSuccess);
			result += tmpResult;
		}

		return result;
	}

	public override Task<int> SaveAsync(
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(null, true, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public override Task<int> SaveAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, true, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public override Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, true, options, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public override Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, acceptAllChangesOnSuccess, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public override async Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		SaveOptions? options,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);

		//if (IsTransactionCommittedDelegate != null)
		//{
		//	var isCommitted = IsTransactionCommittedDelegate();
		//	if (isCommitted)
		//		throw new InvalidOperationException("The underlying transaction has already been committed.");
		//}

		var auditCorrelationId = Guid.NewGuid();
		var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId, options, traceInfo);

		var result = await base.SaveWithoutScopeAsync(acceptAllChangesOnSuccess, options, cancellationToken);

		if (0 < auditEntriesWithTempProperty.Count)
		{
			OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
			var tmpResult = await base.SaveWithoutScopeAsync(acceptAllChangesOnSuccess, options, cancellationToken);
			result += tmpResult;
		}

		return result;
	}

	private List<AuditEntryInternal> OnBeforeSaveChanges(Guid auditCorrelationId, SaveOptions? options, ITraceInfo? traceInfo)
	{
		ChangeTracker.DetectChanges();

		var auditEntries = new List<AuditEntryInternal>();

		var nowUtc = DateTime.UtcNow;
		foreach (var entry in ChangeTracker.Entries())
		{
			if (entry.Entity is IAuditEntry || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
				continue;

			var postModifiedProperties = new List<string>();

			if ((options == null || options.SetConcurrencyToken != false) && entry.Entity is IConcurrent concurrent)
			{
				switch (entry.State)
				{
					case EntityState.Added:
					case EntityState.Modified:
						concurrent.ConcurrencyToken = Guid.NewGuid();
						postModifiedProperties.Add(nameof(concurrent.ConcurrencyToken));
						break;

					default:
						break;
				}
			}

			if ((options == null || options.SetSyncToken != false) && entry.Entity is ISynchronizable synchronizable)
			{
				switch (entry.State)
				{
					case EntityState.Added:
						if (Guid.Empty.Equals(synchronizable.SyncToken))
						{
							synchronizable.SyncToken = Guid.NewGuid();
							postModifiedProperties.Add(nameof(synchronizable.SyncToken));
						}
						break;
					case EntityState.Modified:
						if (WasModifiedNotIgnorredProperty(entry, synchronizable))
						{
							synchronizable.SyncToken = Guid.NewGuid();
							postModifiedProperties.Add(nameof(synchronizable.SyncToken));
						}
						break;

					default:
						break;
				}
			}

			if ((options == null || options.SetCorrelationId != false) && entry.Entity is ICorrelable correlable)
			{
				switch (entry.State)
				{
					case EntityState.Added:
						if (Guid.Empty.Equals(correlable.CorrelationId))
						{
							correlable.CorrelationId = Guid.NewGuid();
							postModifiedProperties.Add(nameof(correlable.CorrelationId));
						}
						break;
					case EntityState.Modified:
						var originalCorrelationId = entry.OriginalValues.GetValue<Guid>(nameof(correlable.CorrelationId));
						if (!correlable.CorrelationId.Equals(originalCorrelationId))
						{
							correlable.CorrelationId = originalCorrelationId;
							postModifiedProperties.Add(nameof(correlable.CorrelationId));
						}
						break;

					default:
						break;
				}
			}

			if (entry.Entity is IAuditable auditable)
			{
				switch (entry.State)
				{
					case EntityState.Added:
						auditable.AuditCreatedUtc = nowUtc;
						auditable.IdAuditCreatedBy = (traceInfo ?? _applicationContext.TraceInfo).IdUser;
						break;

					case EntityState.Modified:
						if (entry.Properties.Any(x => x.IsModified))
						{
							auditable.AuditModifiedUtc = nowUtc;
							auditable.IdAuditModifiedBy = (traceInfo ?? _applicationContext.TraceInfo).IdUser;
						}
						break;

					default:
						break;
				}
			}

			var auditEntry = new AuditEntryInternal(entry)
			{
				IdUser = (traceInfo ?? _applicationContext.TraceInfo).IdUser,
				CreatedUtc = nowUtc,
				CorrelationId = (traceInfo ?? _applicationContext.TraceInfo).CorrelationId,
				CommandQueryName = this.CommandQueryName,
				IdCommandQuery = this.IdCommandQuery
			};

			auditEntries.Add(auditEntry);
			foreach (var property in entry.Properties)
			{
				if (property.IsTemporary)
				{
					auditEntry.TemporaryProperties.Add(property);
					continue;
				}

				string propertyName = property.Metadata.Name;
				if (property.Metadata.IsPrimaryKey())
				{
					auditEntry.KeyValues[propertyName] = property.CurrentValue;
					continue;
				}

				var isPostModified = postModifiedProperties.Contains(propertyName);

				switch (entry.State)
				{
					case EntityState.Added:
						auditEntry.DbOperation = DbOperation.Insert;
						auditEntry.NewValues[propertyName] = property.CurrentValue;
						break;

					case EntityState.Deleted:
						auditEntry.DbOperation = DbOperation.Delete;
						auditEntry.OldValues[propertyName] = property.OriginalValue;
						break;

					case EntityState.Modified:
						if (property.IsModified || isPostModified)
						{
							auditEntry.ChangedColumns.Add(propertyName);
							auditEntry.DbOperation = DbOperation.Update;
							auditEntry.OldValues[propertyName] = property.OriginalValue;
							auditEntry.NewValues[propertyName] = property.CurrentValue;
						}
						break;
				}
			}
		}

		//insert entries without TemporaryProperties
		foreach (var auditEntry in auditEntries.Where(ae => !ae.HasTemporaryProperties))
		{
			AuditEntry.Add(auditEntry.ToAudit<TAuditEntry>(auditCorrelationId));
		}

		return auditEntries.Where(ae => ae.HasTemporaryProperties).ToList();
	}

	private void OnAfterSaveChanges(Guid auditCorrelationId, List<AuditEntryInternal> auditEntriesWithTempProperty)
	{
		foreach (var auditEntry in auditEntriesWithTempProperty)
		{
			foreach (var prop in auditEntry.TemporaryProperties)
			{
				if (prop.Metadata.IsPrimaryKey())
				{
					auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
				}
				else
				{
					auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
				}
			}

			AuditEntry.Add(auditEntry.ToAudit<TAuditEntry>(auditCorrelationId));
		}
	}

	protected override void SetEntities(SaveOptions? options)
	{
		//do nothing
	}
}
