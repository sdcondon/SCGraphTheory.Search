using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;

namespace SCGraphTheory.Search.Local
{
    [TestClass]
    public class HillClimbTests
    {
        public static IEnumerable<object[]> GetBasicTestCases()
        {
            static object[] MakeBasicTestCase(GridGraph<int> graph, (int X, int Y) source, (int X, int Y)[] expectedSteps)
            {
                return new object[] { graph, source, expectedSteps };
            }

            return new object[][]
            {
                MakeBasicTestCase(
                    graph: new GridGraph<int>(new[,]
                    {
                        { 0, 1, 1 },
                        { 1, 2, 1 },
                        { 1, 1, 1 },
                    }),
                    source: (1, 1),
                    expectedSteps: new[] { (1, 1) }),
                MakeBasicTestCase(
                    graph: new GridGraph<int>(new[,]
                    {
                        { 2, 1, 1 },
                        { 1, 0, 1 },
                        { 1, 1, 1 },
                    }),
                    source: (1, 1),
                    expectedSteps: new[] { (0, 0), (0, 0) }),
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetBasicTestCases), DynamicDataSourceType.Method)]
        public void BasicTests(GridGraph<int> graph, (int X, int Y) source, (int X, int Y)[] expectedSteps)
        {
            var search = new HillClimb<GridGraph<int>.Node, GridGraph<int>.Edge, int>(
                source: graph[source.X, source.Y],
                getUtility: n => n.Value);

            Assert.AreEqual(source, search.Current.Coordinates);

            for (int i = 0; i < expectedSteps.Length; i++)
            {
                Assert.IsTrue(search.IsMoving);
                search.NextStep();
                Assert.AreEqual(expectedSteps[i], search.Current.Coordinates);
            }

            Assert.IsFalse(search.IsMoving);
        }
    }
}
