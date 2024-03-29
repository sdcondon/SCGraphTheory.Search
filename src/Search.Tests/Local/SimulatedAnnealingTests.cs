﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;

namespace SCGraphTheory.Search.Local
{
    /// <remarks>
    /// NB: Simulated Annealing is stochastic. So these will occasionally fail. We could use a fake Random-derived class that is actually
    /// deterministic so that it will always pass (for the current implementation), but that would make the test unfair and brittle in the face
    /// of implementation changes.
    /// <para/>
    /// We could make the test stochastic instead - repeat it, and mark as a pass once it hits an acceptable pass rate. But not worth it. This'll do.
    /// </remarks>
    public static class SimulatedAnnealingTests
    {
        private record TestCase(ALGridGraph<int> graph, (int X, int Y) source, (int X, int Y) expectedEnd);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new ALGridGraph<int>(new[,]
                    {
                        { 0, 0, 0 },
                        { 0, 1, 0 },
                        { 0, 0, 0 },
                    }),
                    source: (1, 1),
                    expectedEnd: (1, 1)),
                new TestCase(
                    graph: new ALGridGraph<int>(new[,]
                    {
                        { 1, 0, 1 },
                        { 0, 0, 2 },
                        { 1, 2, 3 },
                    }),
                    source: (0, 0),
                    expectedEnd: (2, 2)),
            })
            .When(tc =>
            {
                var search = new SimulatedAnnealing<ALGridGraph<int>.Node, ALGridGraph<int>.Edge>(
                    source: tc.graph[tc.source.X, tc.source.Y],
                    getUtility: n => n.Value,
                    annealingSchedule: t => Math.Max(1 - (.01f * t), 0));

                while (!search.IsConcluded)
                {
                    search.NextStep();
                }

                return search;
            })
            .ThenReturns()
            .And((tc, search) => search.Current.Coordinates.Should().Be(tc.expectedEnd, "*most* of the time it should find the global maximum (but note this is a stochastic algorithm - the occasional failure is to be expected)"));
    }
}
