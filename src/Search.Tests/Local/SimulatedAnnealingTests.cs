using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Local
{
    /// <remarks>
    /// NB: Simulated Annealing is stochastic. So these will occasionally fail. We could use a fake Random-derived class that is actually
    /// deterministic so that it will always pass (for the current implementation), but that would make the test unfair and brittle in the face
    /// of implementation changes.
    /// <para/>
    /// We could make the test stochastic instead - repeat it, and mark as a pass once it hits an acceptable pass rate. But not worth it. This'll do.
    /// </summary>
    [TestClass]
    public class SimulatedAnnealingTests
    {
        public static IEnumerable<object[]> GetBasicTestCases()
        {
            static object[] MakeBasicTestCase(GridGraph<int> graph, (int X, int Y) source, (int X, int Y) expectedEnd)
            {
                return new object[] { graph, source, expectedEnd };
            }

            return new object[][]
            {
                MakeBasicTestCase(
                    graph: new GridGraph<int>(new[,]
                    {
                        { 0, 0, 0 },
                        { 0, 1, 0 },
                        { 0, 0, 0 },
                    }),
                    source: (1, 1),
                    expectedEnd: (1, 1)),
                MakeBasicTestCase(
                    graph: new GridGraph<int>(new[,]
                    {
                        { 1, 0, 1 },
                        { 0, 0, 2 },
                        { 1, 2, 3 },
                    }),
                    source: (0, 0),
                    expectedEnd: (2, 2)),
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetBasicTestCases), DynamicDataSourceType.Method)]
        public void BasicTests(GridGraph<int> graph, (int X, int Y) source, (int X, int Y) expectedEnd)
        {
            var search = new SimulatedAnnealing<GridGraph<int>.Node, GridGraph<int>.Edge>(
                source: graph[source.X, source.Y],
                getUtility: n => n.Value,
                annealingSchedule: t => Math.Max(1 - (.01f * t), 0));

            Assert.AreEqual(source, search.Current.Coordinates);

            while (!search.IsConcluded)
            {
                search.NextStep();
            }

            Assert.AreEqual(expectedEnd, search.Current.Coordinates, "NB: testing a stochastic algorithm - the occasional failure is to be expected.");
        }
    }
}
