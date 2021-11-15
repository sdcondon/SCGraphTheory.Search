using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using Shouldly;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    public static class BreadthFirstSearchTests
    {
        private record TestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                // NB: we expect the source node to be added to the search tree in the ctor, so that the first
                // step traverses an edge, or the search is immediately complete. This is (admittedly somewhat subjectively)
                // more intuitive behaviour than the first step just adding the source node to the search tree.
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
                    targetId: 3,
                    expectedSteps: new[] { (1, 2), (1, 4), (2, 3) }),
                new TestCase(
                    graph: new LinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: new[] { (1, 2), (1, 4), (2, 3), (4, 5) }),
            })
            .When(tc =>
            {
                var search = new BreadthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId);

                var searchSteps = SearchHelpers.GetStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns((tc, r) => r.searchSteps.ShouldBe(tc.expectedSteps))
            .And((tc, r) => r.search.Target.ShouldBeSameAs(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
