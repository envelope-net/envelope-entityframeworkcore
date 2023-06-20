using System.Reflection;

namespace Envelope.EntityFrameworkCore.Extensions;

internal static class TypeExtensions
{
    internal static bool IsGenericType(this Type type)
    {
        return type.GetTypeInfo().IsGenericType;
    }

    internal static bool IsInterface(this Type type)
    {
        return type.GetTypeInfo().IsInterface;
    }

    internal static bool IsNullableType(this Type type)
    {
        return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    internal static Type GetNonNullableType(this Type type)
    {
        return type.IsNullableType() ? type.GetGenericArguments()[0] : type;
    }

    internal static bool IsValueType(this Type type)
    {
        return type.GetTypeInfo().IsValueType;
    }

    internal static Type FindGenericType(this Type type, Type genericType)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == genericType) return type;
            if (genericType.IsInterface())
            {
                foreach (Type intfType in type.GetInterfaces())
                {
                    Type found = intfType.FindGenericType(genericType);
                    if (found != null) return found;
                }
            }
            type = type.GetTypeInfo().BaseType;
        }
        return null;
    }

    internal static MemberInfo FindPropertyOrField(this Type type, string memberName)
    {
        MemberInfo memberInfo = type.FindPropertyOrField(memberName, false);

        if (memberInfo == null)
        {
            memberInfo = type.FindPropertyOrField(memberName, true);
        }

        return memberInfo;
    }

    internal static MemberInfo FindPropertyOrField(this Type type, string memberName, bool staticAccess)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
            (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
        foreach (Type t in type.SelfAndBaseTypes())
        {
            var members = t.GetProperties(flags)
                .OfType<MemberInfo>()
                .Concat(t.GetFields(flags).OfType<MemberInfo>())
                .Where(m => m.Name.IsCaseInsensitiveEqual(memberName)).ToArray();

            if (members.Length != 0) return members[0];
        }
        return null;
    }

    internal static IEnumerable<Type> SelfAndBaseTypes(this Type type)
    {
        if (type.IsInterface())
        {
            List<Type> types = new List<Type>();
            AddInterface(types, type);
            return types;
        }
        return type.SelfAndBaseClasses();
    }

    internal static IEnumerable<Type> SelfAndBaseClasses(this Type type)
    {
        while (type != null)
        {
            yield return type;
            type = type.GetTypeInfo().BaseType;
        }
    }

    static void AddInterface(List<Type> types, Type type)
    {
        if (!types.Contains(type))
        {
            types.Add(type);
            foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
        }
    }

    internal static PropertyInfo GetIndexerPropertyInfo(this Type type, params Type[] indexerArguments)
    {
        return
            (from p in type.GetProperties()
             where AreArgumentsApplicable(indexerArguments, p.GetIndexParameters())
             select p).FirstOrDefault();
    }

    private static bool AreArgumentsApplicable(IEnumerable<Type> arguments, IEnumerable<ParameterInfo> parameters)
    {
        var argumentList = arguments.ToList();
        var parameterList = parameters.ToList();

        if (argumentList.Count != parameterList.Count)
        {
            return false;
        }

        for (int i = 0; i < argumentList.Count; i++)
        {
            if (parameterList[i].ParameterType != argumentList[i])
            {
                return false;
            }
        }

        return true;
    }

    internal static string GetTypeName(this Type type)
    {
        Type baseType = type.GetNonNullableType();
        string s = baseType.Name;
        if (type != baseType) s += '?';
        return s;
    }

    internal static bool IsCompatibleWith(this Type source, Type target)
    {
        if (source == target) return true;
        if (!target.IsValueType()) return target.IsAssignableFrom(source);
        Type st = source.GetNonNullableType();
        Type tt = target.GetNonNullableType();
        if (st != source && tt == target) return false;

        if (st.IsEnumType() || tt.IsEnumType())
        {
            return st == tt;
        }

        if (st == typeof(sbyte))
        {
            return tt == typeof(sbyte) || tt == typeof(short) ||
                tt == typeof(int) || tt == typeof(long) ||
                tt == typeof(float) || tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(byte))
        {
            return tt == typeof(byte) || tt == typeof(short) || tt == typeof(ushort) || tt == typeof(int) ||
                tt == typeof(uint) || tt == typeof(long) || tt == typeof(ulong) || tt == typeof(float) ||
                tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(short))
        {
            return tt == typeof(short) || tt == typeof(int) ||
                tt == typeof(long) || tt == typeof(float) ||
                tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(ushort))
        {
            return tt == typeof(ushort) || tt == typeof(int) || tt == typeof(uint) ||
                tt == typeof(long) || tt == typeof(ulong) || tt == typeof(float) ||
                tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(int))
        {
            return tt == typeof(int) || tt == typeof(long) ||
                tt == typeof(float) || tt == typeof(double) ||
                tt == typeof(decimal);
        }
        else if (st == typeof(uint))
        {
            return tt == typeof(uint) || tt == typeof(long) || tt == typeof(ulong) ||
                tt == typeof(float) || tt == typeof(double) ||
                tt == typeof(decimal);
        }
        else if (st == typeof(long))
        {
            return tt == typeof(long) || tt == typeof(float) ||
                tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(ulong))
        {
            return tt == typeof(ulong) || tt == typeof(float) ||
                   tt == typeof(double) || tt == typeof(decimal);
        }
        else if (st == typeof(float))
        {
            return tt == typeof(float) || tt == typeof(double);
        }

        return false;
    }

    internal static object DefaultValue(this Type type)
    {
        if (type.IsValueType())
            return Activator.CreateInstance(type);
        return null;
    }

    internal static bool IsEnumType(this Type type)
    {
        return type.GetNonNullableType().GetTypeInfo().IsEnum;
    }
}