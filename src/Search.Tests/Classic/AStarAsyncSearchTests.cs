using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic;

public static class AStarAsyncSearchTests
{
    public static Test GeneralBehaviour => TestThat
        .GivenEachOf(() => new AsyncSearchBehaviourTestCase[]
        {
            new (
                Graph: new AsyncLinqGraph((1, 2, 1)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps: []),

            new (
                Graph: new AsyncLinqGraph((1, 1, 1)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: []),

            new (
                Graph: new AsyncLinqGraph((1, 2, 1), (1, 9, 1), (2, 10, 1), (9, 10, 10)),
                SourceId: 1,
                TargetId: 10,
                ExpectedSteps: [(1, 9), (1, 2), (2, 10)]),
        })
        .AndEachOf(() => new Func<AsyncSearchBehaviourTestCase, Task<IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>>>[]
        {
            async tc => await AStarAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId - n.Id),

            async tc => await AStarAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, double>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId - n.Id),

            async tc => await AStarAsyncSearchWithNonNumericCost<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, SearchHelpers.NonNumericCost>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost),
                getEstimatedCostToTarget: n => new (tc.TargetId - n.Id)),
        })
        .WhenAsync(async (tc, makeSearch) =>
        {
            var search = await makeSearch(tc);
            var searchSteps = await SearchHelpers.GetStepsToCompletionAsync(search);
            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, _, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, _, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
        .AndAsync(async (tc, _, r) => r.search.Target.Should().Be(await tc.Graph.Nodes.SingleOrDefaultAsync(n => n.Id == tc.TargetId)));

    public static Test InfiniteCostBehaviour => TestThat
        .GivenEachOf(() => new AsyncSearchBehaviourTestCase[]
        {
            new ( // edge cost is infinite
                Graph: new AsyncLinqGraph((1, 2, float.PositiveInfinity)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: []),

            new ( // estimate to goal is infinite
                Graph: new AsyncLinqGraph((1, 3, 1), (3, 2, 1)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: []),
        })
        .AndEachOf(() => new Func<AsyncSearchBehaviourTestCase, Task<IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>>>[]
        {
            async tc => await AStarAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId >= n.Id ? tc.TargetId - n.Id : float.PositiveInfinity),

            async tc => await AStarAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, double>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost,
                getEstimatedCostToTarget: n => tc.TargetId >= n.Id ? tc.TargetId - n.Id : float.PositiveInfinity),
        })
        .WhenAsync(async (tc, makeSearch) =>
        {
            var search = await makeSearch(tc);
            await search.CompleteAsync();
            return search;
        })
        .ThenReturns()
        .And((_, _, r) => r.Target.Should().Be(null));
}
