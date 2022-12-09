using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic.Specialized
{
    public static class RecursiveDFSTests
    {
        private record TestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new LinqGraph((1, 2)),
                    sourceId: 1,
                    targetId: 1,
                    expectedSteps: Array.Empty<(int, int)>()),
                new TestCase(
                    graph: new LinqGraph((1, 1)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: Array.Empty<(int, int)>()),
                new TestCase(
                    graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                    sourceId: 1,
                    targetId: 4,
                    expectedSteps: new[] { (1, 2), (2, 4) }),
                new TestCase(
                    graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                    sourceId: 1,
                    targetId: 5,
                    expectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
                new TestCase(
                    graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
            })
            .When(tc =>
            {
                var search = new RecursiveDFS<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId);

                search.Complete();
                return search;
            })
            .ThenReturns()
            .And((tc, r) => r.Visited.Values.Where(e => e != null).Select(e => (e.From.Id, e.To.Id)).Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
