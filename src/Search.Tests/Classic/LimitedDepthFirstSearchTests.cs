﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    [TestClass]
    public class LimitedDepthFirstSearchTests
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
                depthLimit: 1,
                expectedSteps: Array.Empty<(int, int)>(),
                expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),
            MakeBasicTestCase(
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
            MakeBasicTestCase(
                graph: new LinqGraph((1, 3), (1, 2), (2, 3)),
                sourceId: 1,
                targetId: -1,
                depthLimit: 1,
                expectedSteps: new[] { (1, 2), (1, 3) },
                expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Failed),
            MakeBasicTestCase(
                graph: new LinqGraph((1, 2), (2, 4), (1, 3), (3, 4)),
                sourceId: 1,
                targetId: 2,
                depthLimit: 2,
                expectedSteps: new[] { (1, 3), (3, 4), (1, 2) },
                expectedEndState: LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States.Completed),
        };

        [DataTestMethod]
        [DynamicData(nameof(BasicTestCases), DynamicDataSourceType.Property)]
        public void BasicTests(
            LinqGraph graph,
            int sourceId,
            int targetId,
            int depthLimit,
            (int from, int to)[] expectedSteps,
            LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States expectedEndState)
        {
            var search = new LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>(
                source: graph.Nodes.Single(n => n.Id == sourceId),
                isTarget: n => n.Id == targetId,
                depthLimit: depthLimit);

            SearchAssert.ProgressesAsExpected(graph, search, targetId, expectedSteps);
            Assert.AreEqual(expectedEndState, search.State);
        }

        private static object[] MakeBasicTestCase(
            LinqGraph graph,
            int sourceId,
            int targetId,
            int depthLimit,
            (int from, int to)[] expectedSteps,
            LimitedDepthFirstSearch<LinqGraph.Node, LinqGraph.Edge>.States expectedEndState)
        {
            return new object[] { graph, sourceId, targetId, depthLimit, expectedSteps, expectedEndState };
        }
    }
}
