using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class RecursiveDFSTests
{
    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new SearchBehaviourTestCase[]
        {
            new (
                Graph: new LinqGraph((1, 2)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps: Array.Empty<(int, int)>()),
            new (
                Graph: new LinqGraph((1, 1)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: Array.Empty<(int, int)>()),
            new (
                Graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: 4,
                ExpectedSteps: new[] { (1, 2), (2, 4) }),
            new (
                Graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: 5,
                ExpectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
            new (
                Graph: new LinqGraph((1, 2), (1, 3), (2, 4), (3, 5)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: new[] { (1, 2), (2, 4), (1, 3), (3, 5) }),
        })
        .When(tc =>
        {
            var search = new RecursiveDFS<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId);

            search.Complete();
            return search;
        })
        .ThenReturns()
        .And((tc, r) => r.Visited.Values.Where(e => e != null).Select(e => (e.From.Id, e.To.Id)).Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((tc, r) => r.Target.Should().Be(tc.Graph.Nodes.SingleOrDefault(n => n.Id == tc.TargetId)));
}
