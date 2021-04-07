using System.Linq;

namespace MvvmScarletToolkit
{
    public static class QueryExtensions
    {
        public static IQueryable<T> TrySkip<T>(this IQueryable<T> query, int? index)
        {
            if (index is null) // no argument -> return everything
            {
                return query;
            }

            if (index >= 0) // valid indeces start at 0
            {
                return query.Skip(index.Value);
            }
            else
            {
                return query.Skip(int.MaxValue); // return no results
            }
        }

        public static IQueryable<T> TryTake<T>(this IQueryable<T> query, int? count)
        {
            if (count is null) // no argument -> return everything
            {
                return query;
            }

            if (count > 0) // valid counts start at 1
            {
                return query.Take(count.Value);
            }
            else
            {
                return query.Take(0); // return no results
            }
        }

        public static IQueryable<T> TryPage<T>(this IQueryable<T> query, int? index, int? count)
        {
            return query
                .TrySkip(index)
                .TryTake(count);
        }
    }
}
