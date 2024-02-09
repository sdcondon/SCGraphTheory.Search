using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class AStarSearchTests
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
        .AndEachOf(() => new Func<SearchBehaviourTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
        {
            tc => new AStarSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId - n.Id),

            tc => new AStarSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId - n.Id),

            tc => new AStarSearchWithNonNumericCost<LinqGraph.Node, LinqGraph.Edge, SearchHelpers.NonNumericCost>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost),
                getEstimatedCostToTarget: n => new (tc.TargetId - n.Id)),
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
            new ( // edge cost is infinite
                Graph: new LinqGraph((1, 2, float.PositiveInfinity)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new ( // estimate to goal is infinite
                Graph: new LinqGraph((1, 3, 1), (3, 2, 1)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: Array.Empty<(int, int)>()),
        })
        .AndEachOf(() => new Func<SearchBehaviourTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
        {
            tc => new AStarSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId >= n.Id ? tc.TargetId - n.Id : float.PositiveInfinity),

            tc => new AStarSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                source: tc.Graph.Nodes.Single(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId >= n.Id ? tc.TargetId - n.Id : float.PositiveInfinity),
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
