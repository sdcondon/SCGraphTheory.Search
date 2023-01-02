using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;
using System.Numerics;

namespace SCGraphTheory.Search.Classic
{
    public static class DijkstraSearchTests
    {
        private record GeneralBehaviourTestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps);

        public static Test SearchBehaviour => TestThat
            .GivenEachOf(() => new GeneralBehaviourTestCase[]
            {
                new (
                    graph: new LinqGraph((1, 2, 1)),
                    sourceId: 1,
                    targetId: 1,
                    expectedSteps: Array.Empty<(int, int)>()),

                new (
                    graph: new LinqGraph((1, 1, 1)),
                    sourceId: 1,
                    targetId: -1,
                    expectedSteps: Array.Empty<(int, int)>()),

                new (
                    graph: new LinqGraph((1, 2, 1), (1, 3, 2), (2, 4, 4), (3, 4, 2)),
                    sourceId: 1,
                    targetId: 4,
                    expectedSteps: new[] { (1, 2), (1, 3), (3, 4) }),
            })
            .AndEachOf(() => new Func<GeneralBehaviourTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
            {
                tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => e.Cost),

                tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => e.Cost),

                tc => new DijkstraSearchWithNonNumericCost<LinqGraph.Node, LinqGraph.Edge, MyCost>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => new ((int)e.Cost)),
            })
            .When((tc, makeSearch) =>
            {
                var search = makeSearch(tc);
                var searchSteps = SearchHelpers.GetStepsToCompletion(search);
                return new { search, searchSteps };
            })
            .ThenReturns()
            .And((tc, _, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, _, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
            .And((tc, _, r) => r.search.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));

        private record InfiniteCostTestCase(LinqGraph graph, int sourceId, int targetId);

        public static Test InfiniteCostBehaviour => TestThat
            .GivenEachOf(() => new InfiniteCostTestCase[]
            {
                new (
                    graph: new LinqGraph((1, 2, float.PositiveInfinity)),
                    sourceId: 1,
                    targetId: 2),
            })
            .AndEachOf(() => new Func<InfiniteCostTestCase, ISearch<LinqGraph.Node, LinqGraph.Edge>>[]
            {
                tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => e.Cost),

                tc => new DijkstraSearch<LinqGraph.Node, LinqGraph.Edge, double>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => e.Cost),
            })
            .When((tc, makeSearch) =>
            {
                var search = makeSearch(tc);
                search.Complete();

                return search;
            })
            .ThenReturns()
            .And((tc, _, r) => r.Target.Should().Be(null));

        // Obviously a pointless use of a non-numeric struct because it just wraps a number, but that's not what's under test..
        private record struct MyCost(int magnitude)
            : IComparable<MyCost>, IComparisonOperators<MyCost, MyCost, bool>, IAdditionOperators<MyCost, MyCost, MyCost>, IAdditiveIdentity<MyCost, MyCost>
        {
            public static MyCost AdditiveIdentity => new (0);

            public static MyCost operator +(MyCost left, MyCost right) => new (left.magnitude + right.magnitude);

            public static bool operator <(MyCost left, MyCost right) => left.CompareTo(right) < 0;

            public static bool operator >(MyCost left, MyCost right) => left.CompareTo(right) > 0;

            public static bool operator <=(MyCost left, MyCost right) => left.CompareTo(right) <= 0;

            public static bool operator >=(MyCost left, MyCost right) => left.CompareTo(right) >= 0;

            public int CompareTo(MyCost other) => magnitude.CompareTo(other.magnitude);
        }
    }
}
