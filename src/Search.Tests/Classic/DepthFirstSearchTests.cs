using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    public static class DepthFirstSearchTests
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
                    graph: new LinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                    sourceId: 1,
                    targetId: 5,
                    expectedSteps: new[] { (1, 4), (4, 5) }),
                new TestCase(
                    graph: new LinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: new[] { (1, 4), (4, 5), (1, 2), (2, 3) }),
            })
            .When(tc =>
            {
                var search = new DepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId);

                var searchSteps = SearchHelpers.GetStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns()
            .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
            .And((tc, r) => r.search.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
