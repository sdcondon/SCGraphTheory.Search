using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs.Specialized.AndOr
{
    /// <summary>
    /// And-or graph representation of a set of definite clauses of propositional logic (i.e. statements of the form P₁ ∧ P₂ ∧ .. ∧ Pₙ ⇒ Q).
    /// Backward chaining inference can be carried out by searching this graph with the target nodes being those whose symbols are of the propositions that are known to be true.
    /// </summary>
    public class PropositionalLogicGraph
    {
        private readonly Dictionary<string, List<DefiniteClause>> clausesByConsequentSymbol = new ();
        private readonly bool collapseSingleAntecedentClauses;

        private readonly ConcurrentDictionary<string, PropositionNode> propositionNodeCache = new ();
        private readonly ConcurrentDictionary<DefiniteClause, ClauseNode> clauseNodeCache = new ();
        private readonly ConcurrentDictionary<DefiniteClause, ClauseEdge> clauseEdgeCache = new ();
        private readonly ConcurrentDictionary<(DefiniteClause, string), PropositionEdge> propositionEdgeCache = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropositionalLogicGraph"/> class.
        /// </summary>
        /// <param name="clauses">The clauses that comprise the knowledge base.</param>
        /// <param name="collapseSingleAntecedentClauses">A value to indicate whether clauses with a single antecedent should be collapsed so that the two proposition nodes are adjacent (instead of a clause node sitting between them).</param>
        public PropositionalLogicGraph(IEnumerable<DefiniteClause> clauses, bool collapseSingleAntecedentClauses)
        {
            foreach (var clause in clauses)
            {
                if (!clausesByConsequentSymbol.TryGetValue(clause.ConsequentSymbol, out var clausesWithThisConsequentSymbol))
                {
                    clausesWithThisConsequentSymbol = clausesByConsequentSymbol[clause.ConsequentSymbol] = new List<DefiniteClause>();
                }

                clausesWithThisConsequentSymbol.Add(clause);
            }

            this.collapseSingleAntecedentClauses = collapseSingleAntecedentClauses;
        }

        /// <summary>
        /// Interface for nodes of an <see cref="PropositionalLogicGraph"/>.
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {
            /// <summary>
            /// Gets the symbol of the (consequent) proposition that the node represents.
            /// </summary>
            string Symbol { get; }
        }

        /// <summary>
        /// Interface for edges of an <see cref="PropositionalLogicGraph"/>.
        /// </summary>
        public interface IEdge : IEdge<INode, IEdge>
        {
            /// <summary>
            /// Gets the clause explored by the edge.
            /// </summary>
            DefiniteClause Clause { get; }
        }

        /// <summary>
        /// Gets the node of the graph that corresponds to a particular proposition, the outbound edges of which represent clauses (or propositions, if collapseSingleAntecedentClauses is true) that are sufficient for this proposition.
        /// </summary>
        /// <param name="symbol">The symbol of the proposition of the node to retrieve.</param>
        /// <returns>The node of the graph that corresponds to the given proposition.</returns>
        public PropositionNode GetPropositionNode(string symbol) => PropositionNode.Get(this, symbol);

        /// <summary>
        /// Represents a particular proposition and offers outbound edges that represent clauses (or propositions, if collapseSingleAntecedentClauses is true) that are sufficient for this proposition.
        /// </summary>
        [DebuggerDisplay("{Symbol}")]
        public class PropositionNode : INode
        {
            private readonly PropositionalLogicGraph graph;

            private PropositionNode(PropositionalLogicGraph graph, string symbol) => (this.graph, Symbol) = (graph, symbol);

            /// <inheritdoc />
            public string Symbol { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<IEdge> Edges
            {
                get
                {
                    if (graph.clausesByConsequentSymbol.TryGetValue(Symbol, out var clauses))
                    {
                        return graph.clausesByConsequentSymbol[Symbol].Select<DefiniteClause, IEdge>(c =>
                        {
                            if (graph.collapseSingleAntecedentClauses && c.AntecedentSymbols.Count() == 1)
                            {
                                // No point in using a node to represent a set of conjoined edges if there's just one edge.
                                // Search alg needs to be able to cope with that though - hence this being an optional behaviour.
                                return PropositionEdge.Get(graph, this, c, c.AntecedentSymbols.Single());
                            }
                            else
                            {
                                return ClauseEdge.Get(graph, this, c);
                            }

                            //// And add to summary:
                            /// NB: While there are many similarities between this class and <see cref="ErraticVacuumWorldGraph"/>, this one
                            /// is slightly more complex in that it omits the "and" node when it is not necessary (i.e. where we have "sets" of conjoined
                            /// edges of size 1, i.e. clauses with a single antecedent) - resulting in a graph that doesn't necessarily alternate between
                            /// "or" nodes and "and" nodes.
                        }).ToList();
                    }

                    return new List<IEdge>();
                }
            }

            /// <summary>
            /// Gets the node that corresponds to a given symbol.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="graph">The graph containing the node to retrieve.</param>
            /// <param name="symbol">The symbol to get the corresponding node for.</param>
            /// <returns>The node that corresponds to the given symbol.</returns>
            // NB: Don't need to validate whether we know about this symbol. Closed-world assumption.
            internal static PropositionNode Get(PropositionalLogicGraph graph, string symbol) => graph.propositionNodeCache.GetOrAdd(symbol, s => new PropositionNode(graph, s));
        }

        /// <summary>
        /// Represents a given clause and offers outbound edges that to nodes that represent the antecedent propositions of that clause.
        /// In the standard and-or graph representation, this node type actually represents a set of edges conjoined with an arc.
        /// <para/>
        /// Note the explicit interface implementation here for Edges - so that we can give the
        /// concretely-typed version of the edges collection the same name, which keeps things nice and simple for consumers.
        /// </summary>
        [DebuggerDisplay("{Clause}")]
        public class ClauseNode : INode
        {
            private readonly PropositionalLogicGraph graph;

            private ClauseNode(PropositionalLogicGraph graph, DefiniteClause clause) => (this.graph, Clause) = (graph, clause);

            /// <inheritdoc />
            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;

            /// <summary>
            /// Gets the clause that this node corresponds to.
            /// </summary>
            public DefiniteClause Clause { get; }

            /// <inheritdoc />
            public string Symbol => Clause.ConsequentSymbol;

            /// <summary>
            /// Gets the (concretely-typed) collection of edges that are outbound from this node.
            /// </summary>
            public IReadOnlyCollection<PropositionEdge> Edges
            {
                get
                {
                    return Clause.AntecedentSymbols.Select(s => PropositionEdge.Get(graph, this, Clause, s)).ToList();
                }
            }

            /// <summary>
            /// Gets the node that corresponds to a given clause.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="graph">The graph containing the node to retrieve.</param>
            /// <param name="clause">The clause to get the corresponding node for.</param>
            /// <returns>The node that corresponds to the given clause.</returns>
            internal static ClauseNode Get(PropositionalLogicGraph graph, DefiniteClause clause) => graph.clauseNodeCache.GetOrAdd(clause, c => new ClauseNode(graph, c));
        }

        /// <summary>
        /// Represents the relationship between a consequent proposition and a clause.
        /// In the standard and-or graph representation, this edge type actually represents a set of edges conjoined with an arc.
        /// Connects from a <see cref="PropositionNode"/> to an <see cref="ClauseNode"/>.
        /// <para/>
        /// Note the explicit interface implementation here for From and To - so that we can give the
        /// concretely-typed version of these properties the same names, which keeps things nice
        /// and simple for consumers.
        /// </summary>
        [DebuggerDisplay("From: {From}, To: {To}")]
        public class ClauseEdge : IEdge
        {
            private ClauseEdge(PropositionNode from, ClauseNode to)
            {
                From = from;
                To = to;
            }

            /// <inheritdoc />
            public DefiniteClause Clause => To.Clause;

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects from.
            /// </summary>
            public PropositionNode From { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects to.
            /// </summary>
            public ClauseNode To { get; }

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.From => From;

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.To => To;

            /// <summary>
            /// Gets the edge that represents the relationship between a consequent symbol and an antecedent clause.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="graph">The graph containing the edge to retrieve.</param>
            /// <param name="from">The symbol node that the edge connects from.</param>
            /// <param name="clause">The antecedent clause.</param>
            /// <returns>The edge that represents the relationship between a consequent symbol and an antecedent clause.</returns>
            internal static ClauseEdge Get(PropositionalLogicGraph graph, PropositionNode from, DefiniteClause clause) => graph.clauseEdgeCache.GetOrAdd(clause, t => new ClauseEdge(from, ClauseNode.Get(graph, clause)));
        }

        /// <summary>
        /// Represents the relationship between a clause and one of its antecedent propositions, or between a proposition and a proposition that is sufficient for it.
        /// <para/>
        /// Note the explicit interface implementation here for the To property - so that we can give the
        /// concretely-typed version of this property the same name, which keeps things nice and simple for consumers.
        /// </summary>
        [DebuggerDisplay("From: {From}, To: {To}")]
        public class PropositionEdge : IEdge
        {
            private PropositionEdge(INode from, DefiniteClause clause, PropositionNode to)
            {
                From = from;
                Clause = clause;
                To = to;
            }

            /// <inheritdoc />
            public DefiniteClause Clause { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects to.
            /// </summary>
            public PropositionNode To { get; }

            /// <inheritdoc />
            public INode From { get; }

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.To => To;

            /// <summary>
            /// Gets the edge that represents the relationship between a clause and one of its antecedent symbols, or between symbol and a symbol that is sufficient for it.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="graph">The graph containing the edge to retrieve.</param>
            /// <param name="from">The node that the edge connects from.</param>
            /// <returns>The edge that corresponds to the given outcome of the given action from the given state.</returns>
            internal static PropositionEdge Get(PropositionalLogicGraph graph, INode from, DefiniteClause clause, string symbol) => graph.propositionEdgeCache.GetOrAdd((clause, symbol), t => new PropositionEdge(from, clause, PropositionNode.Get(graph, t.Item2)));
        }

        // TODO: equality to take into account antecedentsymbol elements.. suspect caching aint working properly at the mo as a result.
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "False positive. Is a record.")]
        public record DefiniteClause(IEnumerable<string> AntecedentSymbols, string ConsequentSymbol)
        {
            /// <inheritdoc />
            public override string ToString() => $"{string.Join(" ∧ ", AntecedentSymbols)} ⇒ {ConsequentSymbol}";
        }
    }
}
