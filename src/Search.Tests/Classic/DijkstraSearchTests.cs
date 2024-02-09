using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class DijkstraSearchTests
{
    public static Test SearchBehaviour => TestThat
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
                Graph: new LinqGraph((1, 2, 1), (1, 3, 2), (2, 4, 4), (3, 4, 2)),
                SourceId: 1,
                TargetId: 4,
                ExpectedSteps: new[] { (1, 2), (1, 3), (3, 4) }),
        })
        .AndEachOf(() => new Func<SearchBehaviourTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
        {
            tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            tc => new DijkstraSearchWithNonNumericCost<LinqGraph.Node, LinqGraph.Edge, SearchHelpers.NonNumericCost>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost)),
        })
        .When((tc, makeSearch) =>
        {
            var search = makeSearch(tc);
            var searchSteps = SearchHelpers.GetStepsToCompletion(search);
            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, _, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, _, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
        .And((tc, _, r) => r.search.Target.Should().Be(tc.Graph.Nodes.SingleOrDefault(n => n.Id == tc.TargetId)));

    public static Test InfiniteCostBehaviour => TestThat
        .GivenEachOf(() => new SearchBehaviourTestCase[]
        {
            new (
                Graph: new LinqGraph((1, 2, float.PositiveInfinity)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: []),
        })
        .AndEachOf(() => new Func<SearchBehaviourTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
        {
            tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),
        })
        .When((tc, makeSearch) =>
        {
            var search = makeSearch(tc);
            search.Complete();

            return search;
        })
        .ThenReturns()
        .And((_, _, r) => r.Target.Should().Be(null));
}
