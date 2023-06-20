using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Expressions;

public abstract class ExpressionBuilderBase
{
	private readonly ExpressionBuilderOptions options;
	private readonly Type itemType;
	private ParameterExpression parameterExpression;

	protected ExpressionBuilderBase(Type itemType)
	{
		this.itemType = itemType;
		options = new ExpressionBuilderOptions();
	}

	public ExpressionBuilderOptions Options => options;

	protected internal Type ItemType => itemType;

	public ParameterExpression ParameterExpression
	{
		get
		{
			return parameterExpression ??= Expression.Parameter(ItemType, "item");
		}
		set
		{
			parameterExpression = value;
		}
	}
}