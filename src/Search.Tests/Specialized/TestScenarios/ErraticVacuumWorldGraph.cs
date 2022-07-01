using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Specialized.TestScenarios
{
    /// <summary>
    /// A graph representation of the "erratic vacuum world" scenario from §4.3 of "Artifical Intelligence: A Modern Approach".
    /// This is an example of an "and-or tree". It also serves as an example of a graceful way to implement a graph that includes
    /// multiple node and edge types.
    /// </summary>
    public class ErraticVacuumWorldGraph
    {
        /// <summary>
        /// Interface for nodes of an <see cref="ErraticVacuumWorldGraph"/>.
        /// </summary>
        public interface INode : INode<INode, IEdge>
        {
            /// <summary>
            /// Gets the current state of the system.
            /// </summary>
            public State State { get; }
        }

        /// <summary>
        /// Interface for edges of an <see cref="ErraticVacuumWorldGraph"/>.
        /// </summary>
        public interface IEdge : IEdge<INode, IEdge>
        {
            /// <summary>
            /// Gets the current state of the system.
            /// </summary>
            public State State { get; }

            /// <summary>
            /// Gets the action being carried out.
            /// </summary>
            public Actions Action { get; }
        }

        /// <summary>
        /// Gets the node of the graph that corresponds to a particular state, the outbound edges of which represent possible actions that can be taken.
        /// </summary>
        /// <param name="state">The state of the node to retrieve.</param>
        /// <returns>The node of the graph that corresponds to the given state.</returns>
        public static StateNode GetStateNode(State state) => StateNode.Get(state);

        /// <summary>
        /// Represents a given state and offers outbound edges that represent possible actions.
        /// This is an "or" node in the and-or tree parlance.
        /// </summary>
        public class StateNode : INode
        {
            private static readonly ConcurrentDictionary<State, StateNode> Cache = new ();

            private StateNode(State state)
            {
                State = state;
            }

            /// <inheritdoc />
            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;

            /// <inheritdoc />
            public State State { get; }

            /// <summary>
            /// Gets the (concretely-typed) collection of edges that are outbound from this node.
            /// </summary>
            public IReadOnlyCollection<ActionEdge> Edges => State.GetAvailableActions().Select(a => ActionEdge.Get(this, a)).ToList().AsReadOnly();

            /// <summary>
            /// Gets the node that corresponds to a given state.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="state">The state to get the corresponding node for.</param>
            /// <returns>The node that corresponds to the given state.</returns>
            public static StateNode Get(State state) => Cache.GetOrAdd(state, s => new StateNode(s));
        }

        /// <summary>
        /// Represents a given action from a given state and offers outbound edges that represent possible outcomes.
        /// This is an "and" node in the and-or tree parlance.
        /// </summary>
        public class ActionNode : INode
        {
            private static readonly ConcurrentDictionary<(State state, Actions action), ActionNode> ActionNodeCache = new ();

            private ActionNode(State state, Actions action)
            {
                State = state;
                Action = action;
            }

            /// <inheritdoc />
            IReadOnlyCollection<IEdge> INode<INode, IEdge>.Edges => Edges;

            /// <inheritdoc />
            public State State { get; }

            /// <summary>
            /// Gets the action being carried out.
            /// </summary>
            public Actions Action { get; }

            /// <summary>
            /// Gets the (concretely-typed) collection of edges that are outbound from this node.
            /// </summary>
            public IReadOnlyCollection<OutcomeEdge> Edges => State.GetPossibleOutcomes(Action).Select(s => OutcomeEdge.Get(this, s)).ToList().AsReadOnly();

            /// <summary>
            /// Gets the node that corresponds to a given state and action.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="state">The state to get the corresponding node for.</param>
            /// <param name="action">The action to get the corresponding node for.</param>
            /// <returns>The node that corresponds to the given state and action.</returns>
            public static ActionNode Get(State state, Actions action) => ActionNodeCache.GetOrAdd((state, action), t => new ActionNode(t.state, t.action));
        }

        /// <summary>
        /// Represents a given action from a given state. Connects from a <see cref="StateNode"/> to an <see cref="ActionNode"/>.
        /// </summary>
        public class ActionEdge : IEdge
        {
            private static readonly ConcurrentDictionary<(State state, Actions action), ActionEdge> Cache = new ();

            private ActionEdge(StateNode from, ActionNode to)
            {
                State = from.State;
                Action = to.Action;
                From = from;
                To = to;
            }

            /// <inheritdoc />
            public State State { get; }

            /// <inheritdoc />
            public Actions Action { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects from.
            /// </summary>
            public StateNode From { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects to.
            /// </summary>
            public ActionNode To { get; }

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.From => From;

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.To => To;

            /// <summary>
            /// Gets the edge that corresponds to a given action from a given state.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="from">The state node that the edge connects from.</param>
            /// <param name="action">The action being carried out.</param>
            /// <returns>The edge that corresponds to the given action from the given state.</returns>
            public static ActionEdge Get(StateNode from, Actions action) => Cache.GetOrAdd((from.State, action), t => new ActionEdge(from, ActionNode.Get(t.state, t.action)));
        }

        /// <summary>
        /// Represents the outcome of an action from a given state. Connects from an <see cref="ActionNode"/> to a <see cref="StateNode"/>.
        /// </summary>
        public class OutcomeEdge : IEdge
        {
            private static readonly ConcurrentDictionary<(State, Actions, State outcome), OutcomeEdge> Cache = new ();

            private OutcomeEdge(ActionNode from, StateNode to)
            {
                From = from;
                To = to;
            }

            /// <inheritdoc />
            public State State { get; }

            /// <inheritdoc />
            public Actions Action { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects from.
            /// </summary>
            public ActionNode From { get; }

            /// <summary>
            /// Gets the (concretely-typed) node that this edge connects to.
            /// </summary>
            public StateNode To { get; }

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.From => From;

            /// <inheritdoc />
            INode IEdge<INode, IEdge>.To => From;

            /// <summary>
            /// Gets the edge that corresponds to a given outcome of a given action from a given state.
            /// <para/>
            /// NB: Constructor access is restricted to allow for enforcement of caching.
            /// Would have been fine just to create new short-lived instances as the graph is navigated - esp given that this is just for test use - but couldn't bring myself to do it.
            /// </summary>
            /// <param name="from">The action node that the edge connects from.</param>
            /// <param name="outcome">The outcome of the action.</param>
            /// <returns>The edge that corresponds to the given outcome of the given action from the given state.</returns>
            public static OutcomeEdge Get(ActionNode from, State outcome) => Cache.GetOrAdd((from.State, from.Action, outcome), t => new OutcomeEdge(from, StateNode.Get(t.outcome)));
        }

        /// <summary>
        /// A container for the current state of the erratic vacuum world.
        /// </summary>
        /// <param name="VacuumPosition">The current position of the vacuum.</param>
        /// <param name="IsCurrentLocationDirty">A value indicating whether the current position of the vacuum is dirty.</param>
        /// <param name="IsOtherLocationDirty">A value indicating whether the other position is dirty.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "False positive. Is a record.")]
        public record State(VacuumPositions VacuumPosition, bool IsCurrentLocationDirty, bool IsOtherLocationDirty)
        {
            /// <summary>
            /// Gets a value indicating whether the "left" location is dirty.
            /// </summary>
            public bool IsLeftDirty => (VacuumPosition == VacuumPositions.Left && IsCurrentLocationDirty) || (VacuumPosition == VacuumPositions.Right && IsOtherLocationDirty);

            /// <summary>
            /// Gets a value indicating whether the "right" location is dirty.
            /// </summary>
            public bool IsRightDirty => (VacuumPosition == VacuumPositions.Right && IsCurrentLocationDirty) || (VacuumPosition == VacuumPositions.Left && IsOtherLocationDirty);

            /// <summary>
            /// Gets an enumerable of the available actions from this state.
            /// </summary>
            /// <returns>An enumerable of the available actions from this state.</returns>
            public IEnumerable<Actions> GetAvailableActions()
            {
                // Can always suck:
                yield return Actions.Suck;

                // Can move right in the left position and left in the right position.
                // NB: just having a "move" action would probably make things simpler, but this is how
                // the problem is formulated in the source material, and I don't want to deviate too much.
                yield return VacuumPosition switch
                {
                    VacuumPositions.Left => Actions.Right,
                    VacuumPositions.Right => Actions.Left,
                    _ => throw new InvalidOperationException(),
                };
            }

            /// <summary>
            /// Gets an enumerable of the possible outcomes of a given action from this state.
            /// </summary>
            /// <param name="action">The action to get the possible outcomes of.</param>
            /// <returns>An enumerable of the possible outcomes of a given action from this state.</returns>
            public IEnumerable<State> GetPossibleOutcomes(Actions action)
            {
                if (action == Actions.Suck)
                {
                    if (IsCurrentLocationDirty)
                    {
                        // Might clean just this location:
                        yield return this with { IsCurrentLocationDirty = false };

                        // Might clean other location too:
                        if (IsOtherLocationDirty)
                        {
                            yield return this with { IsCurrentLocationDirty = false, IsOtherLocationDirty = false };
                        }
                    }
                    else
                    {
                        // Might leave everything unchanged:
                        yield return this;

                        // Might deposit dirt:
                        yield return this with { IsCurrentLocationDirty = true };
                    }
                }
                else if (action == Actions.Left)
                {
                    // Always moves:
                    // NB: a production version of this would probably want to check the current location.
                    yield return this with { VacuumPosition = VacuumPositions.Left, IsCurrentLocationDirty = IsOtherLocationDirty, IsOtherLocationDirty = IsCurrentLocationDirty };
                }
                else if (action == Actions.Right)
                {
                    // Always moves:
                    // NB: a production version of this would probably want to check the current location.
                    yield return this with { VacuumPosition = VacuumPositions.Right, IsCurrentLocationDirty = IsOtherLocationDirty, IsOtherLocationDirty = IsCurrentLocationDirty };
                }
            }
        }

        /// <summary>
        /// Enumeration of possible vacuum positions.
        /// </summary>
        public enum VacuumPositions
        {
            /// <summary>The vacuum is in the "left" position.</summary>
            Left,

            /// <summary>The vacuum is in the "right" position.</summary>
            Right,
        }

        /// <summary>
        /// Enumeration of possible actions.
        /// </summary>
        public enum Actions
        {
            /// <summary>Move to the left.</summary>
            Left,

            /// <summary>Move to the right.</summary>
            Right,

            /// <summary>Suck up some dirt.</summary>
            Suck,
        }
    }
}
