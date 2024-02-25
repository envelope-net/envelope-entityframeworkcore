using Envelope.EntityFrameworkCore.Queries.Includes;
using Envelope.Exceptions;
using Envelope.Queries;
using Envelope.Queries.Includes;

namespace Envelope.EntityFrameworkCore.Queries;

public class QueryableBuilder<T> : Envelope.Queries.QueryableBuilder<T>, IQueryableBuilder<T>, IQueryModifier<T>
	where T : class
{
	public QueryableBuilder()
		: base()
	{
	}

	public override Envelope.Queries.QueryableBuilder<T> Includes(Action<IIncludeBaseDescriptorBuilder<T>> include)
	{
		Throw.ArgumentNull(include);

		var builder = new IncludeBaseDescriptorBuilder<T>();
		include.Invoke(builder);
		Modify(builder);

		return this;
	}
}
