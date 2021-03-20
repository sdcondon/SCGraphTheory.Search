using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCGraphTheory.Search.Classic;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search
{
    internal static class SearchAssert
    {
        public static void ProgressesAsExpected(
            Graph graph,
            ISearch<Graph.Node, Graph.Edge> search,
            int expectedTargetId,
            (int from, int to)[] expectedSteps)
        {
            var expectedSearchTree = new Dictionary<int, int>();
            for (int i = 0; i < expectedSteps.Length; i++)
            {
                expectedSearchTree[expectedSteps[i].to] = expectedSteps[i].from;
                Assert.IsFalse(search.IsConcluded);
                search.NextStep();
                CollectionAssert.AreEquivalent(
                    expectedSearchTree.Select(kvp => (kvp.Value, kvp.Key)).ToArray(),
                    search.Visited.Values.Where(ke => !ke.IsOnFrontier && ke.Edge != null).Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id)).ToArray());
            }

            Assert.IsTrue(search.IsConcluded);
            Assert.AreEqual(graph.Nodes.SingleOrDefault(n => n.Id == expectedTargetId), search.Target);
        }
    }
}
