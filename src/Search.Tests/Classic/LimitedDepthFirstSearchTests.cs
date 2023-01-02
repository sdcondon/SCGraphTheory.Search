﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    public static class LimitedDepthFirstSearchTests
    {
        private record TestCase(LinqGraph graph, int sourceId, int targetId, int depthLimit, (int from, int to)[] expectedSteps, LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States expectedEndState);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new TestCase[]
            {
                new (
                    graph: new LinqGraph((1, 2)),
                    sourceId: 1,
                    targetId: 1,
                    depthLimit: 1,
                    expectedSteps: Array.Empty<(int, int)>(),
                    expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),

                new (
                    graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                    sourceId: 1,
                    targetId: -1,
                    depthLimit: 1,
                    expectedSteps: new[] { (1, 3), (1, 2) },
                    expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.CutOff),

                // NB: By expecting the following to be failed rather than cutoff, we are expecting our implementation to keep track
                // of cutoff nodes in case they are eventually hit via a shorter path (as opposed to maintaining a single
                // boolean to indicate that a cutoff has occured - as in many reference implementations I have seen).
                // Of course, keeping track does require more memory (a hashset instead of a bool) and time to do so - and
                // I'm not sure whether its "worth it" in the general case (obviously not worth it for a tree, for example).
                // For later consideration I guess:
                new (
                    graph: new LinqGraph((1, 3), (1, 2), (2, 3)),
                    sourceId: 1,
                    targetId: -1,
                    depthLimit: 1,
                    expectedSteps: new[] { (1, 2), (1, 3) },
                    expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Failed),

                new (
                    graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                    sourceId: 1,
                    targetId: 2,
                    depthLimit: 2,
                    expectedSteps: new[] { (1, 3), (3, 4), (1, 2) },
                    expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),
            })
            .When(tc =>
            {
                var search = new LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    depthLimit: tc.depthLimit);

                var searchSteps = SearchHelpers.GetStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns()
            .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
            .And((tc, r) => r.search.State.Should().Be(tc.expectedEndState))
            .And((tc, r) => r.search.Target.Should().BeSameAs(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));
    }
}
