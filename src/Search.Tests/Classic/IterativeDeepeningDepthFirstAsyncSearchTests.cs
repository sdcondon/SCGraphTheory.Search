using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class IterativeDeepeningDepthFirstAsyncSearchTests
{
    private record TestCase(AsyncLinqGraph Graph, int SourceId, int TargetId, (int from, int to)[][] ExpectedSteps)
    {
        public override string ToString()
        {
            return $"{{ Graph: {string.Join(", ", Graph.Edges.Select(e => $"({e.From.Id}, {e.To.Id})").ToListAsync().AsTask().GetAwaiter().GetResult())}, Source: {SourceId}, Target: {TargetId} }}";
        }
    }

    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new TestCase[]
        {
            new (
                Graph: new AsyncLinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps:
                [
                    []
                ]),

            new (
                Graph: new AsyncLinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps:
                [
                    [],
                    [(1, 3), (1, 2)],
                    [(1, 3), (3, 4), (1, 2)],
                ]),
        })
        .WhenAsync(async tc =>
        {
            var search = new IterativeDeepeningDepthFirstAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId);

            var searchSteps = await SearchHelpers.GetIterativeDeepeningStepsToCompletionAsync(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .AndAsync(async (tc, r) => r.search.Target.Should().Be(await tc.Graph.Nodes.SingleOrDefaultAsync(n => n.Id == tc.TargetId)));
}
