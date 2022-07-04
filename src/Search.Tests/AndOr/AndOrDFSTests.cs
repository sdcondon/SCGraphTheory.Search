using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.Benchmarks.AlternativeSearches.Specialized;
using SCGraphTheory.Search.TestGraphs.SpecificScenarios.AiAModernApproach;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.AndOr
{
    public static class AndOrDFSTests
    {
        public static Test ErraticVaccumWorld => TestThat
            .When(() =>
            {
                var initialState = new ErraticVacuumWorldGraph.State(
                    VacuumPosition: ErraticVacuumWorldGraph.VacuumPositions.Left,
                    IsCurrentLocationDirty: true,
                    IsOtherLocationDirty: true);

                var search = new AndOrDFS<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>(
                    ErraticVacuumWorldGraph.GetStateNode(initialState),
                    n => !n.State.IsLeftDirty && !n.State.IsRightDirty);

                return search.Execute();
            })
            .ThenReturns()
            .And((o) => o.Succeeded.Should().BeTrue())
            .And((o) => o.Tree.Flatten().ToDictionary(kvp => kvp.Key.State, kvp => kvp.Value.Action).Should().BeEquivalentTo(new Dictionary<ErraticVacuumWorldGraph.State, ErraticVacuumWorldGraph.Actions>
            {
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, true, true)] = ErraticVacuumWorldGraph.Actions.Suck,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, false, true)] = ErraticVacuumWorldGraph.Actions.Right,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Right, true, false)] = ErraticVacuumWorldGraph.Actions.Suck,
            }));

        public static Test PropositionalLogic => TestThat
            .When(() =>
            {
                var graph = new PropositionalLogicGraph(new PropositionalLogicGraph.DefiniteClause[]
                {
                    new (new[] { "Q", "R" }, "P"), // P if Q and R
                    new (new[] { "S" }, "P"), // P if S
                    new (new[] { "T" }, "Q"), // Q if T
                    new (new[] { "U" }, "Q"), // Q if U
                });

                var knownTruths = new[] { "U", "R" };

                var search = new AndOrDFS<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>(
                    graph.GetSymbolNode("P"),
                    n => knownTruths.Contains(n.Symbol));

                return search.Execute();
            })
            .ThenReturns()
            .And((o) => o.Succeeded.Should().BeTrue())
            .And((o) => o.Tree.Flatten().ToDictionary(kvp => kvp.Key.Symbol, kvp => kvp.Value.Clause.AntecedentSymbols).Should().BeEquivalentTo(new Dictionary<string, IEnumerable<string>>
            {
                ["P"] = new[] { "Q", "R" }, // P because Q and R
                ["Q"] = new[] { "U" }, // Q because U
                //// ..and U and R because they are the known truths (i.e. the target nodes)
            }));
    }
}
