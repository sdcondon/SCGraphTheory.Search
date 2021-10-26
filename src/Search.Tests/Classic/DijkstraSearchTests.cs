using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using Shouldly;
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
                // NB: we expect the source node to be added to the search tree in the ctor, so that the first
                // step traverses an edge, or the search is immediately complete. This is (admittedly somewhat subjectively)
                // more intuitive behaviour than the first step just adding the source node to the search tree.
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
            .Then((tc, r) => r.searchSteps.ShouldBe(tc.expectedSteps))
            .And((tc, r) => r.search.Target.ShouldBeSameAs(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
