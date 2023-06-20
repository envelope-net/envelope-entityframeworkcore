using Envelope.EntityFrameworkCore.Expressions.MemberAccess;
using Envelope.EntityFrameworkCore.Extensions;
using System.ComponentModel;
using System.Xml;

namespace Envelope.EntityFrameworkCore.Expressions;

public static class ExpressionBuilderFactory
{
	public static MemberAccessExpressionBuilderBase MemberAccess(Type elementType, Type memberType, string memberName)
	{
		memberType ??= typeof(object);

		if (elementType.IsCompatibleWith(typeof(XmlNode)))
		{
			return new XmlNodeChildElementAccessExpressionBuilder(memberName);
		}

		if (elementType.IsCompatibleWith(typeof(ICustomTypeDescriptor)))
		{
			return new CustomTypeDescriptorPropertyAccessExpressionBuilder(elementType, memberType, memberName);
		}

		if (elementType == typeof(object) || elementType.IsCompatibleWith(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
		{
			return new DynamicPropertyAccessExpressionBuilder(elementType, memberName);
		}

		return new PropertyAccessExpressionBuilder(elementType, memberName);
	}

	public static MemberAccessExpressionBuilderBase MemberAccess(IQueryable source, Type memberType, string memberName)
	{
		var builder = MemberAccess(source.ElementType, memberType, memberName);
		builder.Options.LiftMemberAccessToNull = source.Provider.IsLinqToObjectsProvider();

		return builder;
	}
}