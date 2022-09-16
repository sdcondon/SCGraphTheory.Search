using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.AndOr;
using SCGraphTheory.Search.TestGraphs.Specialized.AndOr;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Benchmarks.AlternativeSearches.AndOr
{
    public static class AndOrDFS_SteppableTests
    {
        public static Test ErraticVaccumWorld => TestThat
            .When(() =>
            {
                var initialState = new ErraticVacuumWorldGraph.State(
                    VacuumPosition: ErraticVacuumWorldGraph.VacuumPositions.Left,
                    IsCurrentLocationDirty: true,
                    IsOtherLocationDirty: true);

                var search = new AndOrDFS_Steppable<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>(
                    ErraticVacuumWorldGraph.GetStateNode(initialState),
                    n => !n.State.IsLeftDirty && !n.State.IsRightDirty,
                    e => e is ErraticVacuumWorldGraph.ActionEdge);

                search.Complete();

                return new
                {
                    search.Succeeded,
                    search.Result,
                };
            })
            .ThenReturns()
            .And((o) => o.Succeeded.Should().BeTrue())
            .And((o) => o.Result.Flatten().ToDictionary(kvp => kvp.Key.State, kvp => kvp.Value.Action).Should().BeEquivalentTo(new Dictionary<ErraticVacuumWorldGraph.State, ErraticVacuumWorldGraph.Actions>
            {
                // NB: not the best solution, obviously - this is a (stack-using) depth-first search..
                // Moves dominate because they are the edges AFTER (so added after, so popped before) the suck action.
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, true, true)] = ErraticVacuumWorldGraph.Actions.Right,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Right, true, true)] = ErraticVacuumWorldGraph.Actions.Suck,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Right, false, true)] = ErraticVacuumWorldGraph.Actions.Left,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, true, false)] = ErraticVacuumWorldGraph.Actions.Suck,
            }));

        public static Test PropositionalLogic => TestThat
            .When(() =>
            {
                var graph = new PropositionalLogicGraph(
                    new PropositionalLogicGraph.DefiniteClause[]
                    {
                        new (new[] { "Q", "R" }, "P"), // P if Q and R
                        new (new[] { "S" }, "P"), // P if S
                        new (new[] { "T" }, "Q"), // Q if T
                        new (new[] { "U" }, "Q"), // Q if U
                    },
                    false);

                var knownTruths = new[] { "U", "R" };

                var search = new AndOrDFS_Steppable<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>(
                    graph.GetPropositionNode("P"),
                    n => knownTruths.Contains(n.Symbol),
                    e => e is PropositionalLogicGraph.ClauseEdge);

                search.Complete();

                return new
                {
                    search.Succeeded,
                    search.Result,
                };
            })
            .ThenReturns()
            .And((o) => o.Succeeded.Should().BeTrue())
            .And((o) => o.Result.Flatten().ToDictionary(kvp => kvp.Key.Symbol, kvp => kvp.Value.Clause.AntecedentSymbols).Should().BeEquivalentTo(new Dictionary<string, IEnumerable<string>>
            {
                ["P"] = new[] { "Q", "R" }, // P because Q and R
                ["Q"] = new[] { "U" }, // Q because U
                //// ..and U and R because they are the known truths (i.e. the target nodes)
            }));
    }
}
