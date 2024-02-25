using Envelope.EntityFrameworkCore.Extensions;
using System.Globalization;
using System.Reflection;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

internal static class UnboxT<T>
{
	internal static readonly Func<object, T> Unbox = Create(typeof(T));

	private static Func<object, T> Create(Type type)
	{
		if (!type.IsValueType())
		{
			return ReferenceField;
		}
		if (type.IsGenericType() && !type.GetTypeInfo().IsGenericTypeDefinition && typeof(Nullable<>) == type.GetGenericTypeDefinition())
		{
			MethodInfo nullableFieldMethod = typeof(UnboxT<T>).GetMethod(nameof(UnboxT<T>.NullableField), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo genericMethod = nullableFieldMethod.MakeGenericMethod(new[] { type.GetGenericArguments()[0] });

			return (Func<object, T>)genericMethod.CreateDelegate(typeof(Func<object, T>));
		}
		return ValueField;
	}

	private static TElem? NullableField<TElem>(object value) where TElem : struct
	{
		if (DBNull.Value == value)
		{
			return null;
		}
		return (TElem?)value;
	}

	private static T? ReferenceField(object value)
	{
		if (DBNull.Value != value)
		{
			return (T)value;
		}
		return default;
	}

	/// <exception cref="InvalidCastException"><c>InvalidCastException</c>.</exception>
	private static T ValueField(object value)
	{
		if (DBNull.Value == value)
		{
			throw new InvalidCastException(
				string.Format(CultureInfo.CurrentCulture, "Type: {0} cannot be casted to Nullable type", typeof(T)));
		}
		return (T)value;
	}
}