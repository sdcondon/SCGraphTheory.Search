﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic;

public static class DijkstraAsyncSearchWithNonNumericCostTests
{
    public static Test GeneralBehaviour => TestThat
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
                Graph: new AsyncLinqGraph((1, 2, 1), (1, 9, 1), (2, 10, 1), (9, 10, 10)),
                SourceId: 1,
                TargetId: 10,
                ExpectedSteps: new[] { (1, 9), (1, 2), (2, 10) }),
        })
        .WhenAsync(async tc =>
        {
            var search = await DijkstraAsyncSearchWithNonNumericCost<AsyncLinqGraph.Node, AsyncLinqGraph.Edge, SearchHelpers.NonNumericCost>.CreateAsync(
                source: await tc.Graph.Nodes.SingleAsync(n => n.Id == tc.SourceId),
                isTarget: n => n.Id == tc.TargetId,
                getEdgeCost: e => new ((int)e.Cost));

            var searchSteps = await SearchHelpers.GetStepsToCompletionAsync(search);

            return new { search, searchSteps };
        })
        .ThenReturns()
        .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.ExpectedSteps))
        .And((_, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
        .AndAsync(async (tc, r) => r.search.Target.Should().Be(await tc.Graph.Nodes.SingleOrDefaultAsync(n => n.Id == tc.TargetId)));
}
