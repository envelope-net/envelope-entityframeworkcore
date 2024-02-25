using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

internal class PropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
{
	public PropertyAccessExpressionBuilder(Type itemType, string memberName) : base(itemType, memberName)
	{
	}

	public override Expression CreateMemberAccessExpression()
	{
		//if no property specified then return the item itself
		if (string.IsNullOrEmpty(MemberName))
		{
			return ParameterExpression;
		}

		return ExpressionFactory.MakeMemberAccess(ParameterExpression, MemberName, Options.LiftMemberAccessToNull);
	}
}
