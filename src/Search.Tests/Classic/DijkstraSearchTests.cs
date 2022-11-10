using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    public static class DijkstraSearchTests
    {
        private record TestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new LinqGraph((1, 2, 1)),
                    sourceId: 1,
                    targetId: 1,
                    expectedSteps: Array.Empty<(int, int)>()),
                new TestCase(
                    graph: new LinqGraph((1, 1, 1)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: Array.Empty<(int, int)>()),
                new TestCase(
                    graph: new LinqGraph((1, 2, 1), (1, 3, 2), (2, 4, 4), (3, 4, 2)),
                    sourceId: 1,
                    targetId: 4,
                    expectedSteps: new[] { (1, 2), (1, 3), (3, 4) }),
            })
            .When(tc =>
            {
                var search = new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => e.Cost);

                var searchSteps = SearchHelpers.GetStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns()
            .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.search.Visited.Values.Where(v => v.Edge != null).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
            .And((tc, r) => r.search.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));


        private record InfiniteCostTestCase(LinqGraph graph, int sourceId, int targetId);

        public static Test InfiniteCostBehaviour => TestThat
                .GivenEachOf(() => new[]
                {
                    new InfiniteCostTestCase(
                        graph: new LinqGraph((1, 2, float.PositiveInfinity)),
                        sourceId: 1,
                        targetId: 2)
                })
                .When(tc =>
                {
                    var search = new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                        source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                        isTarget: n => n.Id == tc.targetId,
                        getEdgeCost: e => e.Cost);

                    search.Complete();

                    return search;
                })
                .ThenReturns()
                .And((tc, r) => r.Target.Should().Be(null));
    }
}
