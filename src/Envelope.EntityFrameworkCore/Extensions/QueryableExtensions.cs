using Envelope.EntityFrameworkCore.Expressions;
using Envelope.EntityFrameworkCore.Expressions.Sorting;
using Envelope.EntityFrameworkCore.Queries;
using Envelope.Exceptions;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class QueryableExtensions
{
	public static IQueryable OrderBy(this IQueryable source, IEnumerable<SortDescriptor> sortDescriptors)
	{
		var builder = new SortDescriptorCollectionExpressionBuilder(source, sortDescriptors);
		return builder.Sort();
	}

	public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<SortDescriptor> sortDescriptors)
	{
		var builder = new SortDescriptorCollectionExpressionBuilder<T>(source, sortDescriptors);
		return builder.Sort();
	}

	public static IQueryable<T> Apply<T>(this IQueryable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T: class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((Envelope.Queries.IQueryModifier<T>)builder).Apply(source);
	}

	public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((Envelope.Queries.IQueryModifier<T>)builder).ApplyIncludes(source);
	}

	public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((Envelope.Queries.IQueryModifier<T>)builder).ApplyPaging(source);
	}

	public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((Envelope.Queries.IQueryModifier<T>)builder).ApplySort(source);
	}

	//public static IEnumerable ToDataSourceResult(
	//	this IEnumerable enumerable,
	//	DataSourceRequest request)
	//{
	//	return enumerable.AsQueryable().ToDataSourceResult(request);
	//}

	//public static Task<IEnumerable> ToDataSourceResultAsync(
	//	this IEnumerable enumerable,
	//	DataSourceRequest request)
	//{
	//	return Task.Run(() => ToDataSourceResult(enumerable, request));
	//}

	//public static Task<IEnumerable> ToDataSourceResultAsync(
	//	this IEnumerable enumerable,
	//	DataSourceRequest request,
	//	CancellationToken cancellation)
	//{
	//	return Task.Run(() => ToDataSourceResult(enumerable, request), cancellation);
	//}

	//public static IQueryable ToDataSourceResult(
	//	this IQueryable queryable,
	//	DataSourceRequest request)
	//{
	//	return queryable.CreateDataSourceResult(request);
	//}

	//public static Task<IQueryable> ToDataSourceResultAsync(
	//	this IQueryable queryable,
	//	DataSourceRequest request)
	//{
	//	return Task.Run(() => ToDataSourceResult(queryable, request));
	//}

	//public static Task<IQueryable> ToDataSourceResultAsync(
	//	this IQueryable queryable,
	//	DataSourceRequest request,
	//	CancellationToken cancellationToken)
	//{
	//	return Task.Run(() => ToDataSourceResult(queryable, request), cancellationToken);
	//}

	//public static IEnumerable<T> ToDataSourceResult<T>(
	//	this IEnumerable<T> enumerable,
	//	DataSourceRequest request)
	//{
	//	return enumerable.AsQueryable().CreateDataSourceResult<T>(request);
	//}

	//public static Task<IEnumerable<T>> ToDataSourceResultAsync<T>(
	//	this IEnumerable<T> enumerable,
	//	DataSourceRequest request)
	//{
	//	return Task.Run(() => ToDataSourceResult(enumerable, request));
	//}

	//public static Task<IEnumerable<T>> ToDataSourceResultAsync<T>(
	//	this IEnumerable<T> enumerable,
	//	DataSourceRequest request,
	//	CancellationToken cancellationToken)
	//{
	//	return Task.Run(() => ToDataSourceResult(enumerable, request), cancellationToken);
	//}

	//public static IQueryable<T> ToDataSourceResult<T>(
	//	this IQueryable<T> enumerable,
	//	DataSourceRequest request)
	//{
	//	return enumerable.CreateDataSourceResult<T>(request);
	//}

	//public static Task<IQueryable<T>> ToDataSourceResultAsync<T>(
	//	this IQueryable<T> queryable,
	//	DataSourceRequest request)
	//{
	//	return Task.Run(() => ToDataSourceResult(queryable, request));
	//}

	//public static Task<IQueryable<T>> ToDataSourceResultAsync<T>
	//	(this IQueryable<T> queryable, DataSourceRequest request, CancellationToken cancellationToken)
	//{
	//	return Task.Run(() => ToDataSourceResult<T>(queryable, request), cancellationToken);
	//}

	//private static IQueryable CreateDataSourceResult(this IQueryable queryable, DataSourceRequest request)
	//{
	//	var result = queryable;

	//	if (request.Sorts?.Any() == true)
	//		result = result.OrderBy(request.Sorts);

	//	return result;
	//}

	//private static IQueryable<T> CreateDataSourceResult<T>(this IQueryable<T> queryable, DataSourceRequest request)
	//{
	//	var result = queryable;

	//	if (request.Sorts?.Any() == true)
	//		result = result.OrderBy(request.Sorts);

	//	return result;
	//}

	//private static IEnumerable Execute(this IQueryable source)
	//{
	//	if (source == null)
	//		throw new ArgumentNullException(nameof(source));

	//	var type = source.ElementType;

	//	var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;

	//	foreach (var item in source)
	//	{
	//		list.Add(item);
	//	}

	//	return list;
	//}

	//private static IEnumerable<T> Execute<T>(this IQueryable<T> source)
	//{
	//	if (source == null)
	//		throw new ArgumentNullException(nameof(source));

	//	var list = new List<T>();

	//	foreach (var item in source)
	//	{
	//		list.Add(item);
	//	}

	//	return list;
	//}
}
