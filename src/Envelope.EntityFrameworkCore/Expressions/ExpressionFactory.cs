using Envelope.EntityFrameworkCore.Expressions.MemberAccess.Tokenizer;
using Envelope.EntityFrameworkCore.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Envelope.EntityFrameworkCore.Expressions;

public static class ExpressionFactory
{
	public static ConstantExpression ZeroExpression
	{
		get { return Expression.Constant(0); }
	}

	public static ConstantExpression EmptyStringExpression
	{
		get { return Expression.Constant(string.Empty); }
	}

	public static Expression DefaltValueExpression(Type type)
	{
		return Expression.Constant(type.DefaultValue(), type);
	}

	public static Expression MakeMemberAccess(Expression instance, string memberName)
	{
		foreach (var token in MemberAccessTokenizer.GetTokens(memberName))
		{
			instance = token.CreateMemberAccessExpression(instance);
		}

		return instance;
	}

	public static Expression MakeMemberAccess(Expression instance, string memberName, bool liftMemberAccessToNull)
	{
		var memberAccess = MakeMemberAccess(instance, memberName);

		if (liftMemberAccessToNull)
		{
			return LiftMemberAccessToNull(memberAccess);
		}

		return memberAccess;
	}

	public static Expression LiftMemberAccessToNull(Expression memberAccess)
	{
		var defaultValueExpression = DefaltValueExpression(memberAccess.Type);
		return LiftMemberAccessToNullRecursive(memberAccess, memberAccess, defaultValueExpression);
	}

	public static Expression LiftMethodCallToNull(Expression instance, MethodInfo method, params Expression[] arguments)
	{
		var memberAccess = ExtractMemberAccessExpressionFromLiftedExpression(instance);
		var callExpression = Expression.Call(memberAccess, method, arguments);

		return LiftMemberAccessToNull(callExpression);
	}

	private static Expression LiftMemberAccessToNullRecursive(Expression memberAccess, Expression conditionalExpression, Expression defaultValue)
	{
		var instance = GetInstanceExpressionFromExpression(memberAccess);
		if (instance == null)
		{
			return conditionalExpression;
		}

		conditionalExpression = CreateIfNullExpression(instance, conditionalExpression, defaultValue);

		return LiftMemberAccessToNullRecursive(instance, conditionalExpression, defaultValue);
	}

	private static Expression? GetInstanceExpressionFromExpression(Expression memberAccess)
	{
		if (memberAccess is MemberExpression memberExpression)
		{
			return memberExpression.Expression;
		}

		if (memberAccess is MethodCallExpression methodCallExpression)
		{
			return methodCallExpression.Object;
		}

		return null;
	}

	private static Expression CreateIfNullExpression(Expression instance, Expression memberAccess, Expression defaultValue)
	{
		if (ShouldGenerateCondition(instance.Type))
		{
			return CreateConditionExpression(instance, memberAccess, defaultValue);
		}

		return memberAccess;
	}

	private static bool ShouldGenerateCondition(Type type)
	{
		return !type.IsValueType() || type.IsNullableType();
	}

	private static Expression CreateConditionExpression(Expression instance, Expression memberAccess, Expression defaultValue)
	{
		var nullExpression = DefaltValueExpression(instance.Type);
		var isNotNullExpression = Expression.NotEqual(instance, nullExpression);

		return Expression.Condition(isNotNullExpression, memberAccess, defaultValue);
	}

	private static Expression ExtractMemberAccessExpressionFromLiftedExpression(Expression liftedToNullExpression)
	{
		while (liftedToNullExpression.NodeType == ExpressionType.Conditional)
		{
			var conditional = (ConditionalExpression)liftedToNullExpression;

			if (conditional.Test.NodeType == ExpressionType.NotEqual)
			{
				liftedToNullExpression = conditional.IfTrue;
			}
			else
			{
				liftedToNullExpression = conditional.IfFalse;
			}
		}

		return liftedToNullExpression;
	}

	/// <exception cref="ArgumentException">Provided expression should have string type</exception>
	internal static Expression LiftStringExpressionToEmpty(Expression stringExpression)
	{
		if (stringExpression.Type != typeof(string))
		{
			throw new ArgumentException("Provided expression should have string type", nameof(stringExpression));
		}

		if (IsNotNullConstantExpression(stringExpression))
		{
			return stringExpression;
		}

		return Expression.Coalesce(stringExpression, EmptyStringExpression);
	}

	internal static bool IsNotNullConstantExpression(Expression expression)
	{
		if (expression.NodeType == ExpressionType.Constant)
		{
			var constantExpression = (ConstantExpression)expression;
			return constantExpression.Value != null;
		}

		return false;
	}
}