using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using Shouldly;
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
        private record TestCase(GridGraph<int> graph, (int X, int Y) source, (int X, int Y) expectedEnd);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new GridGraph<int>(new[,]
                    {
                        { 0, 0, 0 },
                        { 0, 1, 0 },
                        { 0, 0, 0 },
                    }),
                    source: (1, 1),
                    expectedEnd: (1, 1)),
                new TestCase(
                    graph: new GridGraph<int>(new[,]
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
                var search = new SimulatedAnnealing<GridGraph<int>.Node, GridGraph<int>.Edge>(
                    source: tc.graph[tc.source.X, tc.source.Y],
                    getUtility: n => n.Value,
                    annealingSchedule: t => Math.Max(1 - (.01f * t), 0));

                while (!search.IsConcluded)
                {
                    search.NextStep();
                }

                return search;
            })
            .Then((tc, search) => search.Current.Coordinates.ShouldBe(tc.expectedEnd, "NB: testing a stochastic algorithm - the occasional failure is to be expected."));
    }
}
