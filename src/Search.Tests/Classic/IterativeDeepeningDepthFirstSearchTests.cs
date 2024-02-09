using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class IterativeDeepeningDepthFirstSearchTests
{
    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new (
                Graph: new LinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps:
                [
                    []
                ]),

            new (
                Graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps:
                [
                    [(1, 3), (1, 2)],
                    [(1, 3), (3, 4), (1, 2)],
                ]),
        })
        .When(tc =>
        {
            var search = new IterativeDeepeningDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId);

            var searchSteps = SearchHelpers.GetIterativeDeepeningStepsToCompletion(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((tc, r) => r.search.Target.Should().Be(tc.Graph.Nodes.SingleOrDefault(n => n.Id == tc.TargetId)));

    private record TestCase(LinqGraph Graph, int SourceId, int TargetId, (int from, int to)[][] ExpectedSteps)
    {
        public override string ToString()
        {
            return $"{{ Graph: {string.Join(", ", Graph.Edges.Select(e => $"({e.From.Id}, {e.To.Id})"))}, Source: {SourceId}, Target: {TargetId} }}";
        }
    }
}
