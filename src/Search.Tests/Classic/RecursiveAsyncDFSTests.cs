using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class RecursiveAsyncDFSTests
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
                Graph: new AsyncLinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: 4,
                ExpectedSteps: new[] { (1, 2), (2, 4) }),
            new (
                Graph: new AsyncLinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: 5,
                ExpectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
            new (
                Graph: new AsyncLinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
        })
        .When(async tc =>
        {
            var search = new RecursiveAsyncDFS<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId);

            await search.CompleteAsync();
            return search;
        })
        .ThenReturns()
        .And((tc, r) => r.GetAwaiter().GetResult().Visited.Values.Where(e => e != null).Select(e => (e.From.Id, e.To.Id)).Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((tc, r) => r.GetAwaiter().GetResult().Target.Should().Be(tc.Graph.Nodes.SingleOrDefaultAsync(n => n.Id == tc.TargetId).AsTask().GetAwaiter().GetResult()));
}
