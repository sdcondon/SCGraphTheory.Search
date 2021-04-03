using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    [TestClass]
    public class DepthFirstSearchTests
    {
        public static IEnumerable<object[]> BasicTestCases => new object[][]
        {
            // NB: we expect the source node to be added to the search tree in the ctor, so that the first
            // step traverses an edge, or the search is immediately complete. This is (admittedly somewhat subjectively)
            // more intuitive behaviour than the first step just adding the source node to the search tree.
            MakeBasicTestCase(
                graph: new LinqGraph((1, 2)),
                sourceId: 1,
                targetId: 1,
                expectedSteps: Array.Empty<(int, int)>()),
            MakeBasicTestCase(
                graph: new LinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                sourceId: 1,
                targetId: 5,
                expectedSteps: new[] { (1, 4), (4, 5) }),
            MakeBasicTestCase(
                graph: new LinqGraph((1, 2), (2, 3), (1, 4), (4, 5)),
                sourceId: 1,
                targetId: -1,
                expectedSteps: new[] { (1, 4), (4, 5), (1, 2), (2, 3) }),
        };

        [DataTestMethod]
        [DynamicData(nameof(BasicTestCases), DynamicDataSourceType.Property)]
        public void BasicTests(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps)
        {
            var search = new DepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: graph.Nodes.Single(n => n.Id == sourceId),
                isTarget: n => n.Id == targetId);

            SearchAssert.ProgressesAsExpected(graph, search, targetId, expectedSteps);
        }

        private static object[] MakeBasicTestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps)
        {
            return new object[] { graph, sourceId, targetId, expectedSteps };
        }
    }
}
