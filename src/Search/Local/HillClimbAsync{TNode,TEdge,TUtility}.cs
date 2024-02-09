#if NET6_0_OR_GREATER
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Local
{
    /// <summary>
    /// Implementation of the (steepest-ascent) hill climb algorithm that operates on <see cref="IAsyncGraph{TNode, TEdge}"/> instances.
    /// <para/>
    /// See https://en.wikipedia.org/wiki/Hill_climbing for more information on this algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async graph being searched.</typeparam>
    /// <typeparam name="TUtility">The type of the utility measure that this algorithm should use.</typeparam>
    public class HillClimbAsync<TNode, TEdge, TUtility>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
        where TUtility : IComparable<TUtility>
    {
        private readonly Func<TNode, TUtility> getUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="HillClimbAsync{TNode, TEdge, TUtility}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="getUtility">A function for calculating the utility of a given node - which this algorithm attempts to maximise.</param>
        public HillClimbAsync(
            TNode source,
            Func<TNode, TUtility> getUtility)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.getUtility = getUtility ?? throw new ArgumentNullException(nameof(getUtility));
            this.Current = source;
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm moved during the last step (or no steps have been carried out yet).
        /// <para/>
        /// NB: Unlike the classic algorithms in this library, <see cref="HillClimb{TNode, TEdge, TUtility}"/> doesn't "complete". You can continue to call <see cref="NextStepAsync"/> after it stops moving.
        /// This is useful if utilities can change over time - unlike the classic algorithms, the local nature of Hill Climb means it works in this scenario.
        /// </summary>
        public bool IsMoving { get; private set; } = true;

        /// <summary>
        /// Gets the node that is the current one under consideration.
        /// </summary>
        public TNode Current { get; private set; }

        /// <summary>
        /// Executes the next step of the search.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that, if triggered, should cause cancellation of the step. Optional, defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="ValueTask"/> that represents completion of the step.</returns>
        public async ValueTask NextStepAsync(CancellationToken cancellationToken = default)
        {
            var maxUtility = getUtility(Current);
            var maxNode = Current;
            var isMoving = false;
            await foreach (var edge in Current.Edges.WithCancellation(cancellationToken))
            {
                var utility = getUtility(edge.To);
                if (utility.CompareTo(maxUtility) > 0)
                {
                    maxUtility = utility;
                    maxNode = edge.To;
                    isMoving = true;
                }
            }

            Current = maxNode;
            IsMoving = isMoving;
        }
    }
}
#endif
