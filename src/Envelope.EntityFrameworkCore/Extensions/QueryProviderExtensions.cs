namespace Envelope.EntityFrameworkCore.Extensions;

internal static class QueryProviderExtensions
{
    public static bool IsLinqToObjectsProvider(this IQueryProvider provider)
    {
        return provider.GetType().FullName.Contains("EnumerableQuery");
    }
}