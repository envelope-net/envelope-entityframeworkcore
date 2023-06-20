using System.ComponentModel;

namespace Envelope.EntityFrameworkCore.Expressions;

public class SortDescriptor
{
	public string Member { get; set; }

	public ListSortDirection SortDirection { get; set; }
}
