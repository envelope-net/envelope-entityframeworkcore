using System.Globalization;
using System.Xml;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess;

public static class XmlNodeExtensions
{
	/// <exception cref="ArgumentException">
	/// Child element with name specified by <paramref name="childName"/> does not exists.
	/// </exception>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	public static string ChildElementInnerText(this XmlNode node, string childName)
	{
		XmlElement innerElement = node[childName];

		if (innerElement == null)
		{
			string message = string.Format(
				CultureInfo.CurrentCulture, "Child element with specified name: {0} cannot be found.", childName);

			return null;
		}

		return innerElement.InnerText;
	}
}