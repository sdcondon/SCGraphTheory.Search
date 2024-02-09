#if NET6_0_OR_GREATER
using SCGraphTheory.Search.Local.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Local
{
    /// <summary>
    /// An implementation of the simulated annealing algorithm.
    /// <para/>
    /// See https://en.wikipedia.org/wiki/Simulated_annealing for more information on this algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node type of the async graph being searched.</typeparam>
    /// <typeparam name="TEdge">The edge type of the async raph being searched.</typeparam>
    public class SimulatedAnnealingAsync<TNode, TEdge>
        where TNode : IAsyncNode<TNode, TEdge>
        where TEdge : IAsyncEdge<TNode, TEdge>
    {
        private readonly Func<TNode, float> getUtility;
        private readonly Func<int, float> annealingSchedule;
        private readonly Random random = new ();

        private int stepsCarriedOut = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedAnnealingAsync{TNode, TEdge}"/> class.
        /// </summary>
        /// <param name="source">The node to initiate the search from.</param>
        /// <param name="getUtility">A function for calculating the utility of a given node - which this algorithm attempts to maximise.</param>
        /// <param name="annealingSchedule">A function to map the current step count to the "temperature". The probability of making a move that lowers utility is Math.Exp(ΔU / temp).</param>
        public SimulatedAnnealingAsync(
            TNode source,
            Func<TNode, float> getUtility,
            Func<int, float> annealingSchedule)
        {
            // NB: we don't throw for default structs - which could be valid. For example, we could have a struct
            // (backed by some static store) with a single Id field (that happens to have value 0).
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.getUtility = getUtility ?? throw new ArgumentNullException(nameof(getUtility));
            this.annealingSchedule = annealingSchedule ?? throw new ArgumentNullException(nameof(annealingSchedule));
            this.Current = source;
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm will never move again (because the temperature has reached zero and all nodes adjacent to the current node are of lower utility).
        /// <para/>
        /// <para/>
        /// NB: This property is misleading if the annealing schedule can ever increase again after hitting zero (but it is recommended that annealing schedules should monitonically decrease to zero).
        /// </summary>
        public bool IsConcluded { get; private set; }

        /// <summary>
        /// Gets the node that is the current one under consideration.
        /// </summary>
        public TNode Current { get; private set; }

        /// <summary>
        /// Executes the next step of the search.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the completion of the step.</returns>
        public async ValueTask NextStepAsync()
        {
            // ERROR HANDLING: Could perhaps catch OverflowException and wrap in an InvalidOperationException("Max step count reached.", e) for a better error,
            // but its really too much of an edge case to worry about.
            // Alternatively, probably better to throw only if the schedule gives a non-zero temperature for max int..
            var temp = annealingSchedule(stepsCarriedOut++);

            var edgeCount = await Current.Edges.CountAsync();
            var edge = await Current.Edges.ElementAtAsync(random.Next(edgeCount));
            var utilityDelta = getUtility(edge.To) - getUtility(Current);

            if (utilityDelta > 0 || (temp > 0 && random.NextDouble() < Math.Exp(utilityDelta / temp)))
            {
                Current = edge.To;
            }

            IsConcluded = annealingSchedule(stepsCarriedOut) <= 0 && await Current.Edges.AllAsync(e => getUtility(e.To) <= getUtility(Current));
        }
    }
}
#endif
