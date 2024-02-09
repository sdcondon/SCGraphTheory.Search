using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class BreadthFirstAsyncSearchTests
{
    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new AsyncSearchBehaviourTestCase[]
        {
            new (
                Graph: new AsyncLinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new AsyncLinqGraph((1, 1)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new AsyncLinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                SourceId: 1,
                TargetId: 3,
                ExpectedSteps: new[] { (1, 2), (1, 4), (2, 3) }),

            new (
                Graph: new AsyncLinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: new[] { (1, 2), (1, 4), (2, 3), (4, 5) }),
        })
        .When(async tc =>
        {
            var search = new BreadthFirstAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId);

            var searchSteps = await SearchHelpers.GetStepsToCompletionAsync(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.GetAwaiter().GetResult().searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, r) => r.GetAwaiter().GetResult().search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.GetAwaiter().GetResult().searchSteps))
        .And((tc, r) => r.GetAwaiter().GetResult().search.Target.Should().Be(tc.Graph.Nodes.SingleOrDefaultAsync(n => n.Id == tc.TargetId).AsTask().GetAwaiter().GetResult()));
}
