//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Storage;
//using System.Data.Common;

//namespace Envelope.EntityFrameworkCore;

//public static class DbContextTransactionExtensions
//{
//	public static DbTransaction GetDbTransaction(this IDbContextTransaction dbContextTransaction)
//	{
//		if (dbContextTransaction == null)
//			throw new ArgumentNullException(nameof(dbContextTransaction));

//		if (dbContextTransaction is IInfrastructure<DbTransaction> infrastructure)
//			return infrastructure.Instance;

//		throw new NotSupportedException($"Invalid {nameof(IDbContextTransaction)} implementation | type = {dbContextTransaction.GetType().FullName}");
//	}
//}
