using Envelope.Database;
using Envelope.EntityFrameworkCore.Database;
using Envelope.EntityFrameworkCore.QueryCache;
using Envelope.Logging.SerilogEx;
using Envelope.Model.Concurrence;
using Envelope.Model.Correlation;
using Envelope.Model.Synchronyzation;
using Envelope.Trace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Envelope.EntityFrameworkCore;

public abstract class DbContextBase<TIdentity> : Microsoft.EntityFrameworkCore.DbContext, IDbContext
	where TIdentity : struct
{
	protected readonly IApplicationContext<TIdentity> _applicationContext;
	protected readonly ILogger _logger;

	protected DbConnection? ExternalDbConnection { get; private set; }
	protected string? ExternalConnectionString { get; private set; }
	//protected Func<bool>? IsTransactionCommittedDelegate { get; private set; }
	protected internal QueryCacheManager QueryCacheManager { get; private set; }

	private DbConnection? dbConnection;
	public DbConnection DbConnection
	{
		get
		{
			if (dbConnection == null)
				dbConnection = this.Database.GetDbConnection();

			return dbConnection;
		}
	}

	public IDbContextTransaction? DbContextTransaction => Database?.CurrentTransaction;
	public DbTransaction? DbTransaction => Database?.CurrentTransaction?.GetDbTransaction();

	private string? _dbConnectionString;
	public string DBConnectionString
	{
		get
		{

			if (_dbConnectionString == null)
				_dbConnectionString = DbConnection.ConnectionString;

			return _dbConnectionString;
		}
	}

	public string? CommandQueryName { get; internal set; }
	public Guid? IdCommandQuery { get; internal set; }

	public DbContextBase(DbContextOptions options, ILogger logger, IApplicationContext<TIdentity> appContext/*, disabledEtitiesFromAudit, disabledEtityPropertiesFromAudit*/)
		: base(options)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
		QueryCacheManager = new QueryCacheManager(false);
	}

	protected DbContextBase(ILogger logger, IApplicationContext<TIdentity> appContext)
		: base()
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
		QueryCacheManager = new QueryCacheManager(false);
	}

	private bool initialized = false;
	private bool _disposed;
	private readonly object _initLock = new();
	internal void SetExternalConnection(DbConnection? externalDbConnection, string? externalConnectionString/*, Func<bool>? isTransactionCommittedDelegate*/)
	{
		if (initialized)
			return;

		lock (_initLock)
		{
			if (initialized)
				return;

			ExternalDbConnection = externalDbConnection;
			ExternalConnectionString = externalConnectionString;
			//IsTransactionCommittedDelegate = isTransactionCommittedDelegate;

			initialized = true;
		}
	}

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	[Obsolete("Use Save() method instead.", true)]
	public override int SaveChanges()
	{
		throw new NotSupportedException($"Use {nameof(Save)}() method instead.");
	}

	[Obsolete("Use Save(bool acceptAllChangesOnSuccess) method instead.", true)]
	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		throw new NotSupportedException($"Use {nameof(Save)}(bool {nameof(acceptAllChangesOnSuccess)}) method instead.");
	}

	[Obsolete("Use SaveAsync(CancellationToken cancellationToken) method instead.", true)]
	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException($"Use {nameof(SaveAsync)}({nameof(CancellationToken)} {nameof(cancellationToken)}) method instead.");
	}

	[Obsolete("Use SaveAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) method instead.", true)]
	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException($"Use {nameof(SaveAsync)}(bool {nameof(acceptAllChangesOnSuccess)}, {nameof(CancellationToken)} {nameof(cancellationToken)}) method instead.");
	}
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

	public virtual int Save(
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(null, true, null, memberName, sourceFilePath, sourceLineNumber);

	public virtual int Save(
		ITraceInfo traceInfo,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, true, null, memberName, sourceFilePath, sourceLineNumber);

	public virtual int Save(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, true, options, memberName, sourceFilePath, sourceLineNumber);

	public virtual int Save(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> Save(traceInfo, acceptAllChangesOnSuccess, null, memberName, sourceFilePath, sourceLineNumber);

	public virtual int Save(
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

		SetEntities(options);

		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	public virtual Task<int> SaveAsync(
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(null, true, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public virtual Task<int> SaveAsync(
		ITraceInfo traceInfo,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, true, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public virtual Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		SaveOptions? options,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, true, options, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public virtual Task<int> SaveAsync(
		ITraceInfo? traceInfo,
		bool acceptAllChangesOnSuccess,
		CancellationToken cancellationToken = default,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> SaveAsync(traceInfo, acceptAllChangesOnSuccess, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

	public virtual async Task<int> SaveAsync(
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

		SetEntities(options);

		return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	protected int SaveWithoutScope(bool acceptAllChangesOnSuccess)
		=> SaveWithoutScope(acceptAllChangesOnSuccess, null);

	protected int SaveWithoutScope(bool acceptAllChangesOnSuccess, SaveOptions? options)
	{
		SetEntities(options);
		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	protected Task<int> SaveWithoutScopeAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		=> SaveWithoutScopeAsync(acceptAllChangesOnSuccess, null, cancellationToken);

	protected async Task<int> SaveWithoutScopeAsync(bool acceptAllChangesOnSuccess, SaveOptions? options, CancellationToken cancellationToken = default)
	{
		SetEntities(options);
		return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	protected IDisposable CreateDbLogScope(
		string memberName,
		string sourceFilePath,
		int sourceLineNumber)
	{
		var traceFrame =
			new TraceFrameBuilder(_applicationContext.TraceInfo.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.Build();

		var disposable = _logger.BeginScope(new Dictionary<string, object?>
		{
			[nameof(_applicationContext.TraceInfo.TraceFrame)] = traceFrame.ToString(),
			[nameof(_applicationContext.TraceInfo.CorrelationId)] = _applicationContext.TraceInfo.CorrelationId,
			[LogEventHelper.IS_DB_LOG] = true
		});

		return disposable;
	}

	protected static void RegisterUnaccentFunction(ModelBuilder modelBuilder)
	{
		modelBuilder
			.HasDbFunction(() => DbFunc.Unaccent(default))
			.HasName("unaccent");
	}

	protected virtual void SetEntities(SaveOptions? options)
	{
		if (options != null
			&& options.SetConcurrencyToken == false
			&& options.SetSyncToken == false
			&& options.SetCorrelationId == false)
			return;

		ChangeTracker.DetectChanges();

		foreach (var entry in ChangeTracker.Entries())
		{
			if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
				continue;

			if ((options == null || options.SetConcurrencyToken != false) && entry.Entity is IConcurrent concurrent)
			{
				switch (entry.State)
				{
					case EntityState.Added:
					case EntityState.Modified:
						concurrent.ConcurrencyToken = Guid.NewGuid();
						entry.Property(nameof(concurrent.ConcurrencyToken)).IsModified = true;
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
							entry.Property(nameof(synchronizable.SyncToken)).IsModified = true;
						}
						break;
					case EntityState.Modified:
						if (WasModifiedNotIgnorredProperty(entry, synchronizable))
						{
							synchronizable.SyncToken = Guid.NewGuid();
							entry.Property(nameof(synchronizable.SyncToken)).IsModified = true;
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
							entry.Property(nameof(correlable.CorrelationId)).IsModified = true;
						}
						break;
					case EntityState.Modified:
						var originalCorrelationId = entry.OriginalValues.GetValue<Guid>(nameof(correlable.CorrelationId));
						if (!correlable.CorrelationId.Equals(originalCorrelationId))
						{
							correlable.CorrelationId = originalCorrelationId;
							entry.Property(nameof(correlable.CorrelationId)).IsModified = true;
						}
						break;
					default:
						break;
				}
			}
		}
	}

	protected static bool WasModifiedNotIgnorredProperty(EntityEntry entry, ISynchronizable synchronizable)
	{
		if (entry == null || synchronizable == null)
			return false;

		var ignoredProperties = synchronizable.GetIgnoredSynchronizationProperties();
		if (ignoredProperties == null || ignoredProperties.Count == 0)
			return true;

		return entry.Properties.Any(prop => prop.IsModified && !ignoredProperties.Contains(prop.Metadata.Name));
	}

	private readonly object _configureDbContextCacheLock = new();
	public void ConfigureQueryCacheManager(Action<QueryCacheManager> configure, bool force)
	{
		if (configure == null || (!force && QueryCacheManager.IsEnabled))
			return;

		lock (_configureDbContextCacheLock)
		{
			if (force || !QueryCacheManager.IsEnabled)
				configure(QueryCacheManager);
		}
	}

	public void EnableQueryCacheManager()
		=> ConfigureQueryCacheManager(c => c.IsEnabled = true, false);

	public void SetDbTransaction(
		IDbContextTransaction? existingDbContextTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel)
		=> DbContextFactory.SetDbTransaction(
			this,
			existingDbContextTransaction,
			out newDbContextTransaction,
			transactionUsage,
			transactionIsolationLevel);

	public void SetDbTransaction(
		DbTransaction? existingTransaction,
		out IDbContextTransaction? newDbContextTransaction,
		TransactionUsage transactionUsage,
		IsolationLevel? transactionIsolationLevel)
		=> DbContextFactory.SetDbTransaction(
			this,
			existingTransaction,
			out newDbContextTransaction,
			transactionUsage,
			transactionIsolationLevel);

	public override async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await DisposeAsyncCoreAsync().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual async ValueTask DisposeAsyncCoreAsync()
	{
		QueryCacheManager.Dispose();
		await base.DisposeAsync().ConfigureAwait(false);

		QueryCacheManager = null!;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (disposing)
		{
			QueryCacheManager.Dispose();
			base.Dispose();

			QueryCacheManager = null!;
		}
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
