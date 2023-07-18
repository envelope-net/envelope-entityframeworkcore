using Envelope.Exceptions;
using Envelope.Queries;
using Envelope.Queries.Includes;
using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Queries.Includes;

public class IncludeDescriptorBuilder<TEntity, TProperty> : IIncludeDescriptorBuilder<TEntity, TProperty>, IIncludeDescriptorBuilder<TEntity>, IQueryModifier<TEntity>
	where TEntity : class
{
	private readonly IncludeBaseDescriptorBuilder<TEntity> _baseDescriptorBuilder;
	private readonly IncludeDescriptor<TEntity, TProperty> _includeDescriptor;
	private readonly List<IThenIncludeDescriptorBuilder<TEntity, TProperty>> _thenIncludeDescriptorBuilders;

	public IncludeDescriptorBuilder(IncludeBaseDescriptorBuilder<TEntity> baseDescriptorBuilder, IncludeDescriptor<TEntity, TProperty> includeDescriptor)
	{
		Throw.ArgumentNull(baseDescriptorBuilder);
		Throw.ArgumentNull(includeDescriptor);
		_baseDescriptorBuilder = baseDescriptorBuilder;
		_includeDescriptor = includeDescriptor;
		_thenIncludeDescriptorBuilders = new();
	}

	public IIncludeDescriptorBuilder<TEntity, T> IncludeEnumerable<T>(Expression<Func<TEntity, IEnumerable<T>>> memberSelector)
		=> _baseDescriptorBuilder.IncludeEnumerable(memberSelector);

	public IIncludeDescriptorBuilder<TEntity, T> Include<T>(Expression<Func<TEntity, T>> memberSelector)
		=> _baseDescriptorBuilder.Include(memberSelector);

	public IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> ThenIncludeEnumerable<TNextProperty>(Expression<Func<TProperty, IEnumerable<TNextProperty>>> memberSelector)
	{
		Throw.ArgumentNull(memberSelector);

		var thenIncludeDescriptor = _includeDescriptor.SetThenNavigation(memberSelector);
		var thenIncludeDescriptorBuilder = new ThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty>(_baseDescriptorBuilder, thenIncludeDescriptor);
		_thenIncludeDescriptorBuilders.Add(thenIncludeDescriptorBuilder);
		return thenIncludeDescriptorBuilder;
	}

	public IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TProperty, TNextProperty>> memberSelector)
	{
		Throw.ArgumentNull(memberSelector);

		var thenIncludeDescriptor = _includeDescriptor.SetThenNavigation(memberSelector);
		var thenIncludeDescriptorBuilder = new ThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty>(_baseDescriptorBuilder, thenIncludeDescriptor);
		_thenIncludeDescriptorBuilders.Add(thenIncludeDescriptorBuilder);
		return thenIncludeDescriptorBuilder;
	}

	IEnumerable<TEntity> IQueryModifier<TEntity>.Apply(IEnumerable<TEntity> enumerable)
		=> _includeDescriptor.Include(enumerable.AsQueryable());

	IQueryable<TEntity> IQueryModifier<TEntity>.Apply(IQueryable<TEntity> queryable)
		=> _includeDescriptor.Include(queryable);
}
