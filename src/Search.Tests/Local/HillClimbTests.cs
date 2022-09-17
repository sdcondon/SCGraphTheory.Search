using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Local
{
    public static class HillClimbTests
    {
        private record TestCase(ALGridGraph<int> graph, (int X, int Y) source, (int from, int to)[] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new[]
            {
                new TestCase(
                    graph: new ALGridGraph<int>(new[,]
                    {
                        { 0, 1, 1 },
                        { 1, 2, 1 },
                        { 1, 1, 1 },
                    }),
                    source: (1, 1),
                    expectedSteps: new[] { (1, 1) }),
                new TestCase(
                    graph: new ALGridGraph<int>(new[,]
                    {
                        { 2, 1, 1 },
                        { 1, 0, 1 },
                        { 1, 1, 1 },
                    }),
                    source: (1, 1),
                    expectedSteps: new[] { (0, 0), (0, 0) }),
            })
            .When(tc =>
            {
                var search = new HillClimb<ALGridGraph<int>.Node, ALGridGraph<int>.Edge, int>(
                    source: tc.graph[tc.source.X, tc.source.Y],
                    getUtility: n => n.Value);

                var steps = new List<(int, int)>();
                while (search.IsMoving)
                {
                    search.NextStep();
                    steps.Add(search.Current.Coordinates);
                }

                return steps.ToArray();
            })
            .ThenReturns()
            .And((tc, steps) => steps.Should().BeEquivalentTo(tc.expectedSteps));
    }
}
