using Envelope.Exceptions;
using Envelope.Queries;
using Envelope.Queries.Includes;
using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Queries.Includes;

public class ThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> : IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty>, IThenIncludeDescriptorBuilder<TEntity, TProperty>, IQueryModifier<TEntity>
	where TEntity : class
{
	private readonly IncludeBaseDescriptorBuilder<TEntity> _baseDescriptorBuilder;
	private readonly ThenIncludeDescriptor<TEntity, TProperty, TNextProperty> _thenIncludeDescriptor;
	private readonly List<IThenIncludeDescriptorBuilder<TEntity, TNextProperty>> _thenIncludeDescriptorBuilders;

	public ThenIncludeDescriptorBuilder(IncludeBaseDescriptorBuilder<TEntity> baseDescriptorBuilder, ThenIncludeDescriptor<TEntity, TProperty, TNextProperty> thenIncludeDescriptor)
	{
		Throw.ArgumentNull(baseDescriptorBuilder);
		Throw.ArgumentNull(thenIncludeDescriptor);
		_baseDescriptorBuilder = baseDescriptorBuilder;
		_thenIncludeDescriptor = thenIncludeDescriptor;
		_thenIncludeDescriptorBuilders = new();
	}

	public IIncludeDescriptorBuilder<TEntity, T> IncludeEnumerable<T>(Expression<Func<TEntity, IEnumerable<T>>> memberSelector)
		=> _baseDescriptorBuilder.IncludeEnumerable(memberSelector);

	public IIncludeDescriptorBuilder<TEntity, T> Include<T>(Expression<Func<TEntity, T>> memberSelector)
		=> _baseDescriptorBuilder.Include(memberSelector);

	public IThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty> ThenIncludeEnumerable<TNextNestedProperty>(Expression<Func<TNextProperty, IEnumerable<TNextNestedProperty>>> memberSelector)
	{
		Throw.ArgumentNull(memberSelector);

		var thenIncludeDescriptor = _thenIncludeDescriptor.SetThenNavigation(memberSelector);
		var thenIncludeDescriptorBuilder = new ThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty>(_baseDescriptorBuilder, thenIncludeDescriptor);
		_thenIncludeDescriptorBuilders.Add(thenIncludeDescriptorBuilder);
		return thenIncludeDescriptorBuilder;
	}

	public IThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty> ThenInclude<TNextNestedProperty>(Expression<Func<TNextProperty, TNextNestedProperty>> memberSelector)
	{
		Throw.ArgumentNull(memberSelector);

		var thenIncludeDescriptor = _thenIncludeDescriptor.SetThenNavigation(memberSelector);
		var thenIncludeDescriptorBuilder = new ThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty>(_baseDescriptorBuilder, thenIncludeDescriptor);
		_thenIncludeDescriptorBuilders.Add(thenIncludeDescriptorBuilder);
		return thenIncludeDescriptorBuilder;
	}

	IEnumerable<TEntity> IQueryModifier<TEntity>.Apply(IEnumerable<TEntity> enumerable)
	{
		throw new NotSupportedException();
	}

	IQueryable<TEntity> IQueryModifier<TEntity>.Apply(IQueryable<TEntity> queryable)
	{
		throw new NotSupportedException();
	}
}
