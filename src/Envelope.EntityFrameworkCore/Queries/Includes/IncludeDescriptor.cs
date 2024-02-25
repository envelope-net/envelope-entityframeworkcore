using Envelope.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Queries.Includes;

public interface IIncludeDescriptor<TEntity>
	where TEntity : class
{
	IQueryable<TEntity> Include(IQueryable<TEntity> source);
}

public class IncludeDescriptor<TEntity, TProperty> : IIncludeDescriptor<TEntity>
	where TEntity : class
{
	public Expression<Func<TEntity, TProperty>>? NavigationPropertyPath { get; }
	public Expression<Func<TEntity, IEnumerable<TProperty>>>? NavigationEnumerablePropertyPath { get; }
	public IThenIncludeDescriptor<TEntity>? ThenNavigationPropertyPath { get; private set; }
	public IThenIncludeDescriptor<TEntity>? ThenEnumerableNavigationPropertyPath { get; private set; }

	public IncludeDescriptor(Expression<Func<TEntity, TProperty>> navigationPropertyPath)
	{
		Throw.ArgumentNull(navigationPropertyPath);
		NavigationPropertyPath = navigationPropertyPath;
	}

	public IncludeDescriptor(Expression<Func<TEntity, IEnumerable<TProperty>>> navigationPropertyPath)
	{
		Throw.ArgumentNull(navigationPropertyPath);
		NavigationEnumerablePropertyPath = navigationPropertyPath;
	}

	public ThenIncludeDescriptor<TEntity, TProperty, TNextProperty> SetThenNavigation<TNextProperty>(Expression<Func<TProperty, TNextProperty>> thenNavigationPropertyPath)
	{
		if (ThenNavigationPropertyPath != null)
			throw new InvalidOperationException($"{nameof(ThenNavigationPropertyPath)} already set");

		var thenIncludeDescriptor = new ThenIncludeDescriptor<TEntity, TProperty, TNextProperty>(thenNavigationPropertyPath);
		ThenNavigationPropertyPath = thenIncludeDescriptor;
		return thenIncludeDescriptor;
	}

	public ThenIncludeDescriptor<TEntity, TProperty, TNextProperty> SetThenNavigation<TNextProperty>(Expression<Func<TProperty, IEnumerable<TNextProperty>>> thenNavigationPropertyPath)
	{
		if (ThenEnumerableNavigationPropertyPath != null)
			throw new InvalidOperationException($"{nameof(ThenEnumerableNavigationPropertyPath)} already set");

		var thenIncludeDescriptor = new ThenIncludeDescriptor<TEntity, TProperty, TNextProperty>(thenNavigationPropertyPath);
		ThenEnumerableNavigationPropertyPath = thenIncludeDescriptor;
		return thenIncludeDescriptor;
	}

	public IQueryable<TEntity> Include(IQueryable<TEntity> source)
	{
		IQueryable<TEntity> result = NavigationPropertyPath != null
			? source.Include(NavigationPropertyPath)
			: source.Include(NavigationEnumerablePropertyPath!);

		if (ThenNavigationPropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenNavigationPropertyPath.ThenInclude(result);
			else
				result = ThenNavigationPropertyPath.ThenIncludeEnumerable(result);
		}

		if (ThenEnumerableNavigationPropertyPath != null)
		{
			if (NavigationPropertyPath != null)
				result = ThenEnumerableNavigationPropertyPath.ThenInclude(result);
			else
				result = ThenEnumerableNavigationPropertyPath.ThenIncludeEnumerable(result);
		}

		return result;
	}

	IQueryable<TEntity> IIncludeDescriptor<TEntity>.Include(IQueryable<TEntity> source)
		=> Include(source);
}

