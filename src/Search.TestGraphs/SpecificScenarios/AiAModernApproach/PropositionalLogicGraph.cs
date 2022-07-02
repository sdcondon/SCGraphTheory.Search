using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.TestGraphs.SpecificScenarios.AiAModernApproach
{
    /// <summary>
    /// And-or graph representation of a set of definite clauses (i.e. statements of the form P₁ ∧ P₂ ∧ .. ∧ Pₙ ⇒ Q)
    /// of propositional logic. Backward chaining inference can be carried out by searching this graph with the target nodes
    /// being those whose symbols are of the propositions that are known to be true.
    /// </summary>
    public class PropositionalLogicGraph
    {
        private readonly Dictionary<string, List<DefiniteClause>> clausesByConsequentSymbol = new ();

        private readonly ConcurrentDictionary<string, SymbolNode> symbolNodeCache = new ();
        private readonly ConcurrentDictionary<DefiniteClause, ClauseNode> clauseNodeCache = new ();
        private readonly ConcurrentDictionary<DefiniteClause, ClauseEdge> clauseEdgeCache = new ();
        private readonly ConcurrentDictionary<(DefiniteClause, string), SymbolEdge> symbolEdgeCache = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropositionalLogicGraph"/> class.
        /// </summary>
        /// <param name="clauses">The clauses that comprise the knowledge base.</param>
        public PropositionalLogicGraph(IEnumerable<DefiniteClause> clauses)
        {
            foreach (var clause in clauses)
            {
                if (!clausesByConsequentSymbol.TryGetValue(clause.ConsequentSymbol, out var clausesWithThisConsequentSymbol))
                {
                    clausesWithThisConsequentSymbol = clausesByConsequentSymbol[clause.ConsequentSymbol] = new List<DefiniteClause>();
                }

                clausesWithThisConsequentSymbol.Add(clause);
            }
        }

        /// <summary>
        /// Interface for nodes of an <see cref="PropositionalLogicGraph"/>.
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {
            /// <summary>
            /// Gets the (consequent) symbol of the node.
            /// </summary>
            public string Symbol { get; }
        }

        /// <summary>
        /// Interface for edges of an <see cref="PropositionalLogicGraph"/>.
        /// </summary>
        public interface IEdge : IEdge<INode, IEdge>
        {
            /// <summary>
            /// Gets the clause explored by the edge.
            /// </summary>
            public DefiniteClause Clause { get; }
        }

        /// <summary>
        /// Gets the node of the graph that corresponds to a particular propositional symbol, the outbound edges of which represent clauses or symbols that are sufficient for this symbol.
        /// </summary>
        /// <param name="symbol">The symbol of the node to retrieve.</param>
        /// <returns>The node of the graph that corresponds to the given symbol.</returns>
        public SymbolNode GetSymbolNode(string symbol) => SymbolNode.Get(this, symbol);

        /// <summary>
        /// Represents a particular symbol and offers outbound edges that represent clauses or symbols that are sufficient for this symbol.
        /// </summary>
        public class SymbolNode : INode
        {
            private readonly PropositionalLogicGraph graph;

            private SymbolNode(PropositionalLogicGraph graph, string symbol) => (this.graph, Symbol) = (graph, symbol);

            /// <inheritdoc />
            public string Symbol { get; }

            /// <inheritdoc />
            public IReadOnlyCollection<IEdge> Edges
            {
                get
                {
                    if (graph.clausesByConsequentSymbol.TryGetValue(Symbol, out var clauses))
                    {
                        return graph.clausesByConsequentSymbol[Symbol].Select(c =>
                        {
                            return ClauseEdge.Get(graph, this, c);

                            // TODO:
                            ////if (c.AntecedentSymbols.Count() == 1)
                            ////{
                            ////    // No point in using a node to represent a set of conjoined edges if there's just one edge.
                            ////    // Search alg needs to be able to cope with that though.
                            ////    return SymbolEdge.Get(graph, this, c, c.AntecedentSymbols.Single());
                            ////}
                            ////else
                            ////{
                            ////    return ClauseEdge.Get(graph, this, c);
                            ////}
                            ////
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
            internal static SymbolNode Get(PropositionalLogicGraph graph, string symbol) => graph.symbolNodeCache.GetOrAdd(symbol, s => new SymbolNode(graph, s));
        }

        /// <summary>
        /// Represents a given clause and offers outbound edges that represent the antecedent symbols of that clause.
        /// In the standard and-or graph representation, this node type actually represents a set of edges conjoined with an arc.
        /// <para/>
        /// Note the explicit interface implementation here for Edges - so that we can give the
        /// concretely-typed version of the edges collection the same name, which keeps things nice
        /// and simple for consumers.
        /// </summary>
        public class ClauseNode : INode
        {
            private readonly PropositionalLogicGraph graph;

            private ClauseNode(PropositionalLogicGraph graph, DefiniteClause clause) => (this.graph, Clause) = (graph, clause);

            /// <inheritdoc />
            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;

            public DefiniteClause Clause { get; }

            /// <inheritdoc />
            public string Symbol => Clause.ConsequentSymbol;

            /// <summary>
            /// Gets the (concretely-typed) collection of edges that are outbound from this node.
            /// </summary>
            public IReadOnlyCollection<SymbolEdge> Edges
            {
                get
                {
                    return Clause.AntecedentSymbols.Select(s => SymbolEdge.Get(graph, this, Clause, s)).ToList();
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
        /// Represents the relationship between a consequent symbol and an antecedent clause.
        /// In the standard and-or graph representation, this edge type actually represents a set of edges conjoined with an arc.
        /// Connects from a <see cref="SymbolNode"/> to an <see cref="ClauseNode"/>.
        /// <para/>
        /// Note the explicit interface implementation here for From and To - so that we can give the
        /// concretely-typed version of these properties the same names, which keeps things nice
        /// and simple for consumers.
        /// </summary>
        public class ClauseEdge : IEdge
        {
            private ClauseEdge(SymbolNode from, ClauseNode to)
            {
                From = from;
                To = to;
            }

            /// <inheritdoc />
            public DefiniteClause Clause => To.Clause;

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects from.
            /// </summary>
            public SymbolNode From { get; }

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
            /// <returns>The edge that represents the relationship between a consequent symbol and an antecedent clause</returns>
            internal static ClauseEdge Get(PropositionalLogicGraph graph, SymbolNode from, DefiniteClause clause) => graph.clauseEdgeCache.GetOrAdd(clause, t => new ClauseEdge(from, ClauseNode.Get(graph, clause)));
        }

        /// <summary>
        /// Represents the relationship between a clause and one of its antecedent symbols, or between symbol and a symbol that is sufficient for it.
        /// <para/>
        /// Note the explicit interface implementation here for the To property - so that we can give the
        /// concretely-typed version of this property the same name, which keeps things nice
        /// and simple for consumers.
        /// </summary>
        public class SymbolEdge : IEdge
        {
            private SymbolEdge(INode from, DefiniteClause clause, SymbolNode to)
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
            public SymbolNode To { get; }

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
            /// <param name="from">The action node that the edge connects from.</param>
            /// <returns>The edge that corresponds to the given outcome of the given action from the given state.</returns>
            internal static SymbolEdge Get(PropositionalLogicGraph graph, INode from, DefiniteClause clause, string symbol) => graph.symbolEdgeCache.GetOrAdd((clause, symbol), t => new SymbolEdge(from, clause, SymbolNode.Get(graph, t.Item2)));
        }

        // TODO: equality to take into account antecedentsymbol elements.. suspect caching aint working properly at the mo as a result.
        public record DefiniteClause(IEnumerable<string> AntecedentSymbols, string ConsequentSymbol);
    }
}
