namespace Envelope.EntityFrameworkCore.Expressions;

public class ExpressionBuilderOptions
{
	public bool LiftMemberAccessToNull { get; set; }

	public ExpressionBuilderOptions()
	{
		LiftMemberAccessToNull = true;
	}

	public void CopyFrom(ExpressionBuilderOptions other)
	{
		LiftMemberAccessToNull = other.LiftMemberAccessToNull;
	}
}
