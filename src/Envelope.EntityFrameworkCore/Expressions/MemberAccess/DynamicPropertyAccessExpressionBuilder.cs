using Envelope.EntityFrameworkCore.Expressions.MemberAccess.Tokenizer;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

public class DynamicPropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
{
	public DynamicPropertyAccessExpressionBuilder(Type itemType, string memberName)
		: base(itemType, memberName)
	{
	}

	public override Expression CreateMemberAccessExpression()
	{
		//if no property specified then return the item itself
		if (string.IsNullOrEmpty(MemberName))
		{
			return ParameterExpression;
		}

		Expression instance = ParameterExpression;
		foreach (var token in MemberAccessTokenizer.GetTokens(MemberName))
		{
			if (token is PropertyToken propertyToken)
			{
				var propertyName = propertyToken.PropertyName;
				instance = CreatePropertyAccessExpression(instance, propertyName);
			}
			else if (token is IndexerToken indexerToken)
			{
				instance = CreateIndexerAccessExpression(instance, indexerToken);
			}

		}
		return instance;
	}

	private static Expression CreateIndexerAccessExpression(Expression instance, IndexerToken indexerToken)
	{
		var binder =
			Binder.GetIndex(CSharpBinderFlags.None,
				typeof(DynamicPropertyAccessExpressionBuilder),
						new[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(
								CSharpArgumentInfoFlags.Constant
									| CSharpArgumentInfoFlags.UseCompileTimeType, null)
							});

		return DynamicExpression.Dynamic(binder, typeof(object), new[] { instance, indexerToken.Arguments.Select(Expression.Constant).First() });
	}

	private static Expression CreatePropertyAccessExpression(Expression instance, string propertyName)
	{
		var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName,
			typeof(DynamicPropertyAccessExpressionBuilder), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

		return DynamicExpression.Dynamic(binder, typeof(object), new[] { instance });
	}
}