using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    public static class IterativeDeepeningDepthFirstSearchTests
    {
        private record TestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[][] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new LinqGraph((1, 2)),
                    sourceId: 1,
                    targetId: 1,
                    expectedSteps: new (int, int)[][] { Array.Empty<(int, int)>() }),
                new TestCase(
                    graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                    sourceId: 1,
                    targetId: -1,
#pragma warning disable SA1118 // Parameter should not span multiple lines
                    expectedSteps: new (int, int)[][]
                    {
                        new (int, int)[] { (1, 3), (1, 2) },
                        new (int, int)[] { (1, 3), (3, 4), (1, 2) },
                    }),
#pragma warning restore SA1118 // Parameter should not span multiple lines
            })
            .When(tc =>
            {
                var search = new IterativeDeepeningDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId);

                var searchSteps = SearchHelpers.GetIterativeDeepeningStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.search.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
