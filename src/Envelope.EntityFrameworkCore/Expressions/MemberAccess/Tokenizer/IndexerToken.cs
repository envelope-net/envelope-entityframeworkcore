using Envelope.EntityFrameworkCore.Extensions;
using System.Collections.ObjectModel;

namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess.Tokenizer;

internal class IndexerToken : IMemberAccessToken
{
	private readonly ReadOnlyCollection<object> arguments;
	public ReadOnlyCollection<object> Arguments => arguments;

	public IndexerToken(IEnumerable<object> arguments)
	{
		this.arguments = arguments.ToReadOnlyCollection();
	}

	public IndexerToken(params object[] arguments) : this((IEnumerable<object>)arguments)
	{
	}
}