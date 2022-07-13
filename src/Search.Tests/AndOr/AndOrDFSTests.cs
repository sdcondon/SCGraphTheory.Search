﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using SCGraphTheory.Search.TestGraphs.Specialized.AndOr;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                    n => !n.State.IsLeftDirty && !n.State.IsRightDirty,
                    e => e is ErraticVacuumWorldGraph.ActionEdge);

                search.Complete(CancellationToken.None);

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
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, true, true)] = ErraticVacuumWorldGraph.Actions.Suck,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Left, false, true)] = ErraticVacuumWorldGraph.Actions.Right,
                [new (ErraticVacuumWorldGraph.VacuumPositions.Right, true, false)] = ErraticVacuumWorldGraph.Actions.Suck,
            }));

        public static Test PropositionalLogicGraph => TestThat
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

                var search = new AndOrDFS<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>(
                    graph.GetPropositionNode("P"),
                    n => knownTruths.Contains(n.Symbol),
                    e => e is PropositionalLogicGraph.ClauseEdge);

                search.Complete(CancellationToken.None);

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

        private record ValTypeHandlingTestCase(ValLinqGraph graph, int sourceId, int targetId, Dictionary<int, int> expectedTree);

        public static Test ValueTypeHandling => TestThat
            .GivenEachOf(() => new ValTypeHandlingTestCase[]
            {
                new (
                    graph: new ValLinqGraph((0, 1)),
                    sourceId: 0,
                    targetId: 1,
                    expectedTree: new () { [0] = 1 }),

                // Adjacent "or" nodes:
                new (
                    graph: new ValLinqGraph((0, 1), (1, 2)),
                    sourceId: 0,
                    targetId: 2,
                    expectedTree: new () { [0] = 1, [1] = 2 }),
            })
            .When(tc =>
            {
                var search = new AndOrDFS<ValLinqGraph.Node, ValLinqGraph.Edge>(
                    source: tc.graph[tc.sourceId],
                    isTarget: n => n.Id == tc.targetId,
                    isAndEdgeCollection => false);

                search.Complete(CancellationToken.None);

                return new
                {
                    search.Succeeded,
                    search.Result,
                };

            })
            .ThenReturns()
            .And((_, o) => o.Succeeded.Should().BeTrue())
            .And((tc, o) => o.Result.Flatten().ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value.To.Id).Should().BeEquivalentTo(tc.expectedTree));
    }
}
