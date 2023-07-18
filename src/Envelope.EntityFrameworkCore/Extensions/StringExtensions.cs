using System.Globalization;

namespace Envelope.EntityFrameworkCore.Extensions;

public static class StringExtensions
{
	public static string FormatWith(this string instance, params object[] args)
	{
		return string.Format(CultureInfo.CurrentCulture, instance, args);
	}

	public static bool IsCaseInsensitiveEqual(this string instance, string comparing)
	{
		return string.Compare(instance, comparing, StringComparison.OrdinalIgnoreCase) == 0;
	}
}