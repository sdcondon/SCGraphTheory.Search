using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.TestGraphs;
using System;
using System.Linq;
using System.Numerics;

namespace SCGraphTheory.Search.Classic
{
    public static class DijkstraSearchWithNonNumericCostTests
    {
        private record GeneralBehaviourTestCase(LinqGraph graph, int sourceId, int targetId, (int from, int to)[] expectedSteps);

        public static Test GeneralBehaviour => TestThat
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
                    graph: new LinqGraph((1, 2, 1), (1, 9, 1), (2, 10, 1), (9, 10, 10)),
                    sourceId: 1,
                    targetId: 10,
                    expectedSteps: new[] { (1, 9), (1, 2), (2, 10) }),
            })
            .When(tc =>
            {
                var search = new DijkstraSearchWithNonNumericCost<LinqGraph.Node, LinqGraph.Edge, MyCost>(
                    source: tc.graph.Nodes.Single(n => n.Id == tc.sourceId),
                    isTarget: n => n.Id == tc.targetId,
                    getEdgeCost: e => new ((int)e.Cost));

                var searchSteps = SearchHelpers.GetStepsToCompletion(search);

                return new { search, searchSteps };
            })
            .ThenReturns()
            .And((tc, r) => r.searchSteps.Should().BeEquivalentTo(tc.expectedSteps))
            .And((tc, r) => r.search.Visited.Values.Where(v => v.Edge != null && v.IsOnFrontier == false).Select(v => (v.Edge.From.Id, v.Edge.To.Id)).Should().BeEquivalentTo(r.searchSteps))
            .And((tc, r) => r.search.Target.Should().Be(tc.graph.Nodes.SingleOrDefault(n => n.Id == tc.targetId)));

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
