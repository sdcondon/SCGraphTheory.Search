using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search
{
    [TestClass]
    public class DijkstraSearchTests
    {
        public static IEnumerable<object[]> BasicTestCases => new object[][]
        {
            // NB: we expect the source node to be added to the search tree in the ctor, so that the first
            // step traverses an edge, or the search is immediately complete. This is (admittedly somewhat subjectively)
            // more intuitive behaviour than the first step just adding the source node to the search tree.
            MakeBasicTestCase(
                graph: new Graph((1, 2, 1)),
                sourceId: 1,
                targetId: 1,
                expectedSteps: Array.Empty<(int, int)>()),
            MakeBasicTestCase(
                graph: new Graph((1, 2, 1), (1, 3, 2), (2, 4, 4), (3, 4, 2)),
                sourceId: 1,
                targetId: 4,
                expectedSteps: new[] { (1, 2), (1, 3), (3, 4) }),
        };

        [DataTestMethod]
        [DynamicData(nameof(BasicTestCases), DynamicDataSourceType.Property)]
        public void BasicTests(Graph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps)
        {
            var search = new DijkstraSearch<Graph.Node, Graph.Edge>(
                graph.Nodes.Single(n => n.Id == sourceId),
                n => n.Id == targetId,
                e => e.Cost);

            SearchAssert.ProgressesAsExpected(graph, search, targetId, expectedSteps);
        }

        private static object[] MakeBasicTestCase(Graph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps)
        {
            return new object[] { graph, sourceId, targetId, expectedSteps };
        }
    }
}
