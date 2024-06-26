﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic;

public static class DijkstraAsyncSearchTests
{
    public static Test SearchBehaviour => TestThat
        .GivenEachOf(() => new AsyncSearchBehaviourTestCase[]
        {
            new (
                Graph: new AsyncLinqGraph((1, 2, 1)),
                SourceId: 1,
                TargetId: 1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new AsyncLinqGraph((1, 1, 1)),
                SourceId: 1,
                TargetId: -1,
                ExpectedSteps: Array.Empty<(int, int)>()),

            new (
                Graph: new AsyncLinqGraph((1, 2, 1), (1, 3, 2), (2, 4, 4), (3, 4, 2)),
                SourceId: 1,
                TargetId: 4,
                ExpectedSteps: new[] { (1, 2), (1, 3), (3, 4) }),
        })
        .AndEachOf(() => new Func<AsyncSearchBehaviourTestCase, Task<IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>>>[]
        {
            async tc => await DijkstraAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            async tc => await DijkstraAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, double>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            async tc => await DijkstraAsyncSearchWithNonNumericCost<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, SearchHelpers.NonNumericCost>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost)),
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
            new (
                Graph: new AsyncLinqGraph((1, 2, float.PositiveInfinity)),
                SourceId: 1,
                TargetId: 2,
                ExpectedSteps: []),
        })
        .AndEachOf(() => new Func<AsyncSearchBehaviourTestCase, Task<IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>>>[]
        {
            async tc => await DijkstraAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),

            async tc => await DijkstraAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, double>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => e.Cost),
        })
        .When(async (tc, makeSearch) =>
        {
            var search = await makeSearch(tc);
            await search.CompleteAsync();

            return search;
        })
        .ThenReturns()
        .And((_, _, r) => r.GetAwaiter().GetResult().Target.Should().Be(null));
}
