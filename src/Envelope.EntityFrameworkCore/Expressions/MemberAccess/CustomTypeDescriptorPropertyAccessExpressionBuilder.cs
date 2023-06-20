using Envelope.EntityFrameworkCore.Extensions;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

internal class CustomTypeDescriptorPropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
{
	private static readonly MethodInfo PropertyMethod = typeof(CustomTypeDescriptorExtensions).GetMethod(nameof(CustomTypeDescriptorExtensions.Property));

	private readonly Type propertyType;
	public Type PropertyType => propertyType;

	/// <exception cref="ArgumentException"><paramref name="elementType"/> did not implement <see cref="ICustomTypeDescriptor"/>.</exception>
	public CustomTypeDescriptorPropertyAccessExpressionBuilder(Type elementType, Type memberType, string memberName)
		: base(elementType, memberName)
	{
		if (!elementType.IsCompatibleWith(typeof(ICustomTypeDescriptor)))
		{
			throw new ArgumentException(
				string.Format(CultureInfo.CurrentCulture, "ElementType: {0} did not implement {1}", elementType, typeof(ICustomTypeDescriptor)),
				"elementType");
		}

		propertyType = GetPropertyType(memberType);
	}

	private Type GetPropertyType(Type memberType)
	{
		var descriptorProviderPropertyType = GetPropertyTypeFromTypeDescriptorProvider();
		if (descriptorProviderPropertyType != null)
		{
			memberType = descriptorProviderPropertyType;
		}

		//Handle value types for null and DBNull.Value support converting them to Nullable<>
		if (memberType.IsValueType && !memberType.IsNullableType())
		{
			return typeof(Nullable<>).MakeGenericType(memberType);
		}

		return memberType;
	}

	private Type? GetPropertyTypeFromTypeDescriptorProvider()
	{
		var propertyDescriptor = TypeDescriptor.GetProperties(ItemType)[MemberName];
		if (propertyDescriptor != null)
		{
			return propertyDescriptor.PropertyType;
		}

		return null;
	}

	public override Expression CreateMemberAccessExpression()
	{
		ConstantExpression propertyNameExpression = Expression.Constant(MemberName);

		MethodCallExpression propertyExpression =
			Expression.Call(
				PropertyMethod.MakeGenericMethod(propertyType),
				ParameterExpression,
				propertyNameExpression);

		return propertyExpression;
	}
}
