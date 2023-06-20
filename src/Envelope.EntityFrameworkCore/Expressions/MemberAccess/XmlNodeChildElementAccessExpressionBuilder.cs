using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

internal class XmlNodeChildElementAccessExpressionBuilder : MemberAccessExpressionBuilderBase
{
	private static readonly MethodInfo ChildElementInnerTextMethod =
		typeof(XmlNodeExtensions).GetMethod(nameof(XmlNodeExtensions.ChildElementInnerText), new[] { typeof(XmlNode), typeof(string) })!;

	public XmlNodeChildElementAccessExpressionBuilder(string memberName) : base(typeof(XmlNode), memberName)
	{
	}

	public override Expression CreateMemberAccessExpression()
	{
		ConstantExpression childNameExpression = Expression.Constant(MemberName);

		MethodCallExpression childElementInnterTextExtensionMethodExpression =
			Expression.Call(
				ChildElementInnerTextMethod,
				ParameterExpression,
				childNameExpression);

		return childElementInnterTextExtensionMethodExpression;
	}
}