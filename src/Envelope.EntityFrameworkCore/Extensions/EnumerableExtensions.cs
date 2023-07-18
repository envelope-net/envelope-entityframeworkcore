using Envelope.EntityFrameworkCore.Queries;
using Envelope.Exceptions;
using System.Collections.ObjectModel;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class EnumerableExtensions
{
	public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, Action<QueryableBuilder<T>>? queryableBuilder)
		where T : class
	{
		Throw.ArgumentNull(source);

		if (queryableBuilder == null)
			return source;

		var builder = new QueryableBuilder<T>();
		queryableBuilder.Invoke(builder);

		return ((Envelope.Queries.IQueryModifier<T>)builder).Apply(source);
	}

	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
	{
		if (sequence == null)
		{
			return DefaultReadOnlyCollection<T>.Empty;
		}

		if (sequence is ReadOnlyCollection<T> onlys && onlys != null)
			return onlys;

		return new ReadOnlyCollection<T>(sequence.ToArray());
	}

	private static class DefaultReadOnlyCollection<T>
	{
		private static ReadOnlyCollection<T>? _defaultCollection;
		internal static ReadOnlyCollection<T> Empty => _defaultCollection ??= new ReadOnlyCollection<T>(Array.Empty<T>());
	}
}
