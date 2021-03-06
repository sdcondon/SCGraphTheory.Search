﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.TestGraphs;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    internal static class SearchAssert
    {
        public static void ProgressesAsExpected(
            LinqGraph graph,
            ISearch<LinqGraph.Node, LinqGraph.Edge> search,
            int expectedTargetId,
            (int from, int to)[] expectedSteps)
        {
            var expectedExploredEdges = new Dictionary<int, int>();
            for (int i = 0; i < expectedSteps.Length; i++)
            {
                expectedExploredEdges[expectedSteps[i].to] = expectedSteps[i].from;
                Assert.IsFalse(search.IsConcluded);
                search.NextStep();
                CollectionAssert.AreEquivalent(
                    expectedExploredEdges.Select(kvp => (kvp.Value, kvp.Key)).ToArray(),
                    search.Visited.Values.Where(ke => !ke.IsOnFrontier && ke.Edge != null).Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id)).ToArray());
            }

            Assert.IsTrue(search.IsConcluded);
            Assert.AreEqual(graph.Nodes.SingleOrDefault(n => n.Id == expectedTargetId), search.Target);
        }

        public static void IterativeDeepeningProgressesAsExpected(
            LinqGraph graph,
            ISearch<LinqGraph.Node, LinqGraph.Edge> search,
            int expectedTargetId,
            (int from, int to)[][] expectedSteps)
        {
            for (int i = 0; i < expectedSteps.Length; i++)
            {
                var expectedExploredEdges = new Dictionary<int, int>();

                for (int j = 0; j < expectedSteps[i].Length; j++)
                {
                    expectedExploredEdges[expectedSteps[i][j].to] = expectedSteps[i][j].from;
                    Assert.IsFalse(search.IsConcluded);
                    search.NextStep();
                    CollectionAssert.AreEquivalent(
                        expectedExploredEdges.Select(kvp => (kvp.Value, kvp.Key)).ToArray(),
                        search.Visited.Values.Where(ke => !ke.IsOnFrontier && ke.Edge != null).Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id)).ToArray());
                }
            }

            Assert.IsTrue(search.IsConcluded);
            Assert.AreEqual(graph.Nodes.SingleOrDefault(n => n.Id == expectedTargetId), search.Target);
        }
    }
}
