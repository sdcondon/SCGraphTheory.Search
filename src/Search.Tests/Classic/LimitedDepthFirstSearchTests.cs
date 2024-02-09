using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class LimitedDepthFirstSearchTests
{
    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new (
                Graph: new LinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 1,
                DepthLimit: 0,
                ExpectedSteps: [],
                ExpectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),

            new (
                Graph: new LinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 2,
                DepthLimit: 1,
                ExpectedSteps: [(1, 2)],
                ExpectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),

            new (
                Graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                SourceId: 1,
                TargetId: -1,
                DepthLimit: 1,
                ExpectedSteps: [(1, 3), (1, 2)],
                ExpectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.CutOff),

            // NB: By expecting the following to be failed rather than cutoff, we are expecting our implementation to keep track
            // of cutoff nodes in case they are eventually hit via a shorter path (as opposed to maintaining a single
            // boolean to indicate that a cutoff has occured - as in many reference implementations I have seen).
            // Of course, keeping track does require more memory (a hashset instead of a bool) and time to do so - and
            // I'm not sure whether its "worth it" in the general case (obviously not worth it for a tree, for example).
            // And given that much of the point of depth-limited DFS is for memory usage control, this is perhaps a bad idea.
            // Meh. For later consideration I guess:
            new (
                Graph: new LinqGraph((1, 3), (1, 2), (2, 3)),
                SourceId: 1,
                TargetId: -1,
                DepthLimit: 1,
                ExpectedSteps: [(1, 2), (1, 3)],
                ExpectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Failed),

            new (
                Graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                SourceId: 1,
                TargetId: 2,
                DepthLimit: 2,
                ExpectedSteps: [(1, 3), (3, 4), (1, 2)],
                ExpectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),
        })
        .When(tc =>
        {
            var search = new LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                depthLimit: tc.DepthLimit);

            var searchSteps = SearchHelpers.GetStepsToCompletion(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
        .And((tc, r) => r.search.State.Should().Be(tc.ExpectedEndState))
        .And((tc, r) => r.search.Target.Should().BeSameAs(tc.Graph.Nodes.SingleOrDefault(n => n.Id == tc.TargetId)));

    private record TestCase(
        LinqGraph Graph,
        int SourceId,
        int TargetId,
        int DepthLimit,
        (int from, int to)[] ExpectedSteps,
        LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States ExpectedEndState)
    {
        public override string ToString()
        {
            return $"{{ Graph: {string.Join(", ", Graph.Edges.Select(e => $"({e.From.Id}, {e.To.Id})"))}, Source: {SourceId}, Target: {TargetId}, Depth: {DepthLimit} }}";
        }
    }
}
