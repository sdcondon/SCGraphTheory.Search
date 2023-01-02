using System;

namespace SCGraphTheory.Search.Local
{
    /// <summary>
    /// Implementation of the (steepest-ascent) hill climb algorithm.
    /// <para/>
    /// See https://en.wikipedia.org/wiki/Hill_climbing for more information on this algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the graph being searched.</typeparam>
    /// <typeparam name="TUtility">The type of the utility measure that this algorithm should use.</typeparam>
    public class HillClimb<TNode, TEdge, TUtility>
        where TNode : INode<TNode, TEdge>
        where TEdge : IEdge<TNode, TEdge>
        where TUtility : IComparable<TUtility>
    {
        private readonly Func<TNode, TUtility> getUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="HillClimb{TNode, TEdge, TUtility}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="getUtility">A function for calculating the utility of a given node - which this algorithm attempts to maximise.</param>
        public HillClimb(
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
        /// NB: Unlike the classic algorithms in this library, <see cref="HillClimb{TNode, TEdge, TUtility}"/> doesn't "complete". You can continue to call <see cref="NextStep"/> after it stops moving.
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
        public void NextStep()
        {
            var maxUtility = getUtility(Current);
            var maxNode = Current;
            var isMoving = false;
            foreach (var edge in Current.Edges)
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
