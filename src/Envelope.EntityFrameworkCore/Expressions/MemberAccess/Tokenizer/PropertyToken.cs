namespace Envelope.EntityFrameworkCore.Expressions.MemberAccess.Tokenizer;

internal class PropertyToken : IMemberAccessToken
{
	private readonly string propertyName;
	public string PropertyName => propertyName;

	public PropertyToken(string propertyName)
	{
		this.propertyName = propertyName;
	}
}