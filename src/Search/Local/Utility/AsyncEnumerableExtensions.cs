#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Local.Utility
{
    /// <summary>
    /// Extension methods for <see cref="IAsyncEnumerable{T}"/>.
    /// Yes, I could use System.Linq.Async - but I quite like the lack of third-party runtime dependencies.
    /// </summary>
    internal static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Counts the number of elements of an <see cref="IAsyncEnumerable{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the enumerable.</typeparam>
        /// <param name="asyncEnumerable">The enumerable to count.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask"/> that returns the number of elements in the enumerable.</returns>
        public static async ValueTask<int> CountAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            await using var enumerator = asyncEnumerable.ConfigureAwait(false).WithCancellation(cancellationToken).GetAsyncEnumerator();

            int count = 0;
            while (await enumerator.MoveNextAsync())
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Checks whether all of the elements of an enumerable satisfy a given predicate.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the enumerable.</typeparam>
        /// <param name="asyncEnumerable">The enumerable to count.</param>
        /// <param name="predicate">The predicate to evaluate for each element.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A <see cref="ValueTask"/> that returns a value indicating whether all elements of the enumerable satisfy the predicate.</returns>
        public static async ValueTask<bool> AllAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, Func<T, bool> predicate, CancellationToken cancellationToken = default)
        {
            await using var enumerator = asyncEnumerable.ConfigureAwait(false).WithCancellation(cancellationToken).GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync())
            {
                if (!predicate(enumerator.Current))
                {
                    return false;
                }
            }

            return true;
        }

        public static async ValueTask<T> ElementAtAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, int index, CancellationToken cancellationToken = default)
        {
            await using var enumerator = asyncEnumerable.ConfigureAwait(false).WithCancellation(cancellationToken).GetAsyncEnumerator();

            if (index >= 0)
            {
                while (await enumerator.MoveNextAsync())
                {
                    if (index-- == 0)
                    {
                        return enumerator.Current;
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
#endif
