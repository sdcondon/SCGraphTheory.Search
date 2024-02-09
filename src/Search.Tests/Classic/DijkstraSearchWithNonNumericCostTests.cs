using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class DijkstraSearchWithNonNumericCostTests
{
    public static Test GeneralBehaviour => TestThat
        .GivenEachOf(() => new SearchBehaviourTestCase[]
        {
            new (
                Graph: new LinqGraph((1, 2, 1)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new LinqGraph((1, 1, 1)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new LinqGraph((1, 2, 1), (1, 9, 1), (2, 10, 1), (9, 10, 10)),
                SourceId: 1,
                TargetId: 10,
                ExpectedSteps: new[] { (1, 9), (1, 2), (2, 10) }),
        })
        .When(tc =>
        {
            var search = new DijkstraSearchWithNonNumericCost<LinqGraph.Node, LinqGraph.Edge, SearchHelpers.NonNumericCost>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost));

            var searchSteps = SearchHelpers.GetStepsToCompletion(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
        .And((tc, r) => r.search.Target.Should().Be(tc.Graph.Nodes.SingleOrDefault(n => n.Id == tc.TargetId)));
}
