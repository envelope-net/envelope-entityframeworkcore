using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

public abstract class MemberAccessExpressionBuilderBase : ExpressionBuilderBase
{
	private readonly string memberName;
	public string MemberName => memberName;

	protected MemberAccessExpressionBuilderBase(Type itemType, string memberName) : base(itemType)
	{
		this.memberName = memberName;
	}

	public abstract Expression CreateMemberAccessExpression();

	public LambdaExpression CreateLambdaExpression()
	{
		Expression memberExpression = CreateMemberAccessExpression();
		return Expression.Lambda(memberExpression, ParameterExpression);
	}
}