using Envelope.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Queries.Includes;

public interface IThenIncludeDescriptor<TEntity>
	where TEntity : class
{
	IQueryable<TEntity> ThenInclude(IQueryable<TEntity> source);
	IQueryable<TEntity> ThenIncludeEnumerable(IQueryable<TEntity> source);
}

public class ThenIncludeDescriptor<TEntity, TProperty, TNextProperty> : IThenIncludeDescriptor<TEntity>
	where TEntity : class
{
	public Expression<Func<TProperty, TNextProperty>>? NavigationPropertyPath { get; }
	public Expression<Func<TProperty, IEnumerable<TNextProperty>>>? NavigationEnumerablePropertyPath { get; }
	public IThenIncludeDescriptor<TEntity>? ThenNavigationPropertyPath { get; private set; }
	public IThenIncludeDescriptor<TEntity>? ThenNavigationEnumerablePropertyPath { get; private set; }

	public ThenIncludeDescriptor(Expression<Func<TProperty, TNextProperty>> navigationPropertyPath)
	{
		Throw.ArgumentNull(navigationPropertyPath);
		NavigationPropertyPath = navigationPropertyPath;
	}

	public ThenIncludeDescriptor(Expression<Func<TProperty, IEnumerable<TNextProperty>>> navigationPropertyPath)
	{
		Throw.ArgumentNull(navigationPropertyPath);
		NavigationEnumerablePropertyPath = navigationPropertyPath;
	}

	public ThenIncludeDescriptor<TEntity, TNextProperty, TNextNestedProperty> SetThenNavigation<TNextNestedProperty>(Expression<Func<TNextProperty, TNextNestedProperty>> thenNavigationPropertyPath)
	{
		if (ThenNavigationPropertyPath != null)
			throw new InvalidOperationException($"{nameof(ThenNavigationPropertyPath)} already set");

		var thenIncludeDescriptor = new ThenIncludeDescriptor<TEntity, TNextProperty, TNextNestedProperty>(thenNavigationPropertyPath);
		ThenNavigationPropertyPath = thenIncludeDescriptor;
		return thenIncludeDescriptor;
	}

	public ThenIncludeDescriptor<TEntity, TNextProperty, TNextNestedProperty> SetThenNavigation<TNextNestedProperty>(Expression<Func<TNextProperty, IEnumerable<TNextNestedProperty>>> thenNavigationPropertyPath)
	{
		if (ThenNavigationEnumerablePropertyPath != null)
			throw new InvalidOperationException($"{nameof(ThenNavigationEnumerablePropertyPath)} already set");

		var thenIncludeDescriptor = new ThenIncludeDescriptor<TEntity, TNextProperty, TNextNestedProperty>(thenNavigationPropertyPath);
		ThenNavigationEnumerablePropertyPath = thenIncludeDescriptor;
		return thenIncludeDescriptor;
	}

	public IQueryable<TEntity> ThenInclude(IIncludableQueryable<TEntity, TProperty> source)
	{
		IQueryable<TEntity> result = NavigationPropertyPath != null
			? source.ThenInclude(NavigationPropertyPath)
			: source.ThenInclude(NavigationEnumerablePropertyPath!);

		if (ThenNavigationPropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenNavigationPropertyPath.ThenInclude(result);
			else
				result = ThenNavigationPropertyPath.ThenIncludeEnumerable(result);
		}

		if (ThenNavigationEnumerablePropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenNavigationEnumerablePropertyPath.ThenInclude(result);
			else
				result = ThenNavigationEnumerablePropertyPath.ThenIncludeEnumerable(result);
		}

		return result;
	}

	public IQueryable<TEntity> ThenIncludeEnumerable(IIncludableQueryable<TEntity, IEnumerable<TProperty>> source)
	{
		IQueryable<TEntity> result = NavigationPropertyPath != null
			? source.ThenInclude(NavigationPropertyPath)
			: source.ThenInclude(NavigationEnumerablePropertyPath!);

		if (ThenNavigationPropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenNavigationPropertyPath.ThenInclude(result);
			else
				result = ThenNavigationPropertyPath.ThenIncludeEnumerable(result);
		}

		if (ThenNavigationEnumerablePropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenNavigationEnumerablePropertyPath.ThenInclude(result);
			else
				result = ThenNavigationEnumerablePropertyPath.ThenIncludeEnumerable(result);
		}

		return result;
	}

	public IQueryable<TEntity> ThenInclude(IQueryable<TEntity> source)
		=> ThenInclude((IIncludableQueryable<TEntity, TProperty>)source);

	public IQueryable<TEntity> ThenIncludeEnumerable(IQueryable<TEntity> source)
		=> ThenIncludeEnumerable((IIncludableQueryable<TEntity, IEnumerable<TProperty>>)source);
}
