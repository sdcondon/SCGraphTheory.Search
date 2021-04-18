using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    [TestClass]
    public class IterativeDeepeningDepthFirstSearchTests
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
                expectedSteps: Array.Empty<(int, int)[]>()),
            MakeBasicTestCase(
                graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                sourceId: 1,
                targetId: -1,
#pragma warning disable SA1118 // Parameter should not span multiple lines
                expectedSteps: new (int, int)[][]
                {
                    new (int, int)[] { (1, 3), (1, 2) },
                    new (int, int)[] { (1, 3), (3, 4), (1, 2) },
                }),
#pragma warning restore SA1118 // Parameter should not span multiple lines
        };

        [DataTestMethod]
        [DynamicData(nameof(BasicTestCases), DynamicDataSourceType.Property)]
        public void BasicTests(
            LinqGraph graph,
            int sourceId,
            int targetId,
            (int from, int to)[][] expectedSteps)
        {
            var search = new IterativeDeepeningDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: graph.Nodes.Single(n => n.Id == sourceId),
                isTarget: n => n.Id == targetId);

            SearchAssert.IterativeDeepeningProgressesAsExpected(graph, search, targetId, expectedSteps);
        }

        private static object[] MakeBasicTestCase(
            LinqGraph graph,
            int sourceId,
            int targetId,
            (int from, int to)[][] expectedSteps)
        {
            return new object[] { graph, sourceId, targetId, expectedSteps };
        }
    }
}
