using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SCGraphTheory.Search.Classic;

internal static class SearchHelpers
{
    public static (int from, int to)[] GetStepsToCompletion(ISearch<LinqGraph.Node, LinqGraph.Edge> search)
    {
        var searchSteps = new List<(int, int)>();
        while (!search.IsConcluded)
        {
            var exploredEdge = search.NextStep();
            searchSteps.Add((exploredEdge.From.Id, exploredEdge.To.Id));
        }

        return searchSteps.ToArray();
    }

    public static async Task<(int from, int to)[]> GetStepsToCompletionAsync(IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge> search)
    {
        var searchSteps = new List<(int, int)>();

        // for async searches, the first step is expected to "discover" the source node
        await search.NextStepAsync();

        while (!search.IsConcluded)
        {
            var exploredEdge = await search.NextStepAsync();
            searchSteps.Add((exploredEdge.From.Id, exploredEdge.To.Id));
        }

        return searchSteps.ToArray();
    }

    public static (int from, int to)[][] GetIterativeDeepeningStepsToCompletion(ISearch<LinqGraph.Node, LinqGraph.Edge> search)
    {
        var currentIteration = new List<(int, int)>();
        var searchSteps = new List<List<(int, int)>>() { currentIteration };
        while (!search.IsConcluded)
        {
            search.NextStep();

            var exploredEdges = search.Visited.Values
                .Where(ke => !ke.IsOnFrontier && ke.Edge != null)
                .Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id));

            var addedExploredEdges = exploredEdges.Except(currentIteration);
            var removedExploredEdges = currentIteration.Except(exploredEdges);

            if (addedExploredEdges.Count() == 1 && !removedExploredEdges.Any())
            {
                // Same iteration
                currentIteration.Add(addedExploredEdges.Single());
            }
            else
            {
                // New iteration
                currentIteration = [.. exploredEdges];
                searchSteps.Add(currentIteration);
            }
        }

        return searchSteps.Select(s => s.ToArray()).ToArray();
    }

    public static async Task<(int from, int to)[][]> GetIterativeDeepeningStepsToCompletionAsync(IAsyncSearch<AsyncLinqGraph.Node, AsyncLinqGraph.Edge> search)
    {
        // NB: For async searches, the first step (of any given iteration) is expected
        // to "discover" the source node.
        var currentIteration = new List<(int, int)>();
        var searchSteps = new List<List<(int, int)>>() { currentIteration };
        await search.NextStepAsync();
        while (!search.IsConcluded)
        {
            await search.NextStepAsync();

            var exploredEdges = search.Visited.Values
                .Where(ke => !ke.IsOnFrontier && ke.Edge != null)
                .Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id));

            var addedExploredEdges = exploredEdges.Except(currentIteration);
            var removedExploredEdges = currentIteration.Except(exploredEdges);

            if (addedExploredEdges.Count() == 1 && !removedExploredEdges.Any())
            {
                // Same iteration
                currentIteration.Add(addedExploredEdges.Single());
            }
            else
            {
                // New iteration. NB: For async searches, the first step (of any given iteration) is expected
                // to "discover" the source node.
                currentIteration = [];
                searchSteps.Add(currentIteration);
            }
        }

        return searchSteps.Select(s => s.ToArray()).ToArray();
    }

    // Obviously a pointless use of a non-numeric struct because it just wraps a number, but that's not what's under test..
    public record struct NonNumericCost(int Magnitude)
        : IComparable<NonNumericCost>, IComparisonOperators<NonNumericCost, NonNumericCost, bool>, IAdditionOperators<NonNumericCost, NonNumericCost, NonNumericCost>, IAdditiveIdentity<NonNumericCost, NonNumericCost>
    {
        public static NonNumericCost AdditiveIdentity => new (0);

        public static NonNumericCost operator +(NonNumericCost left, NonNumericCost right) => new (left.Magnitude + right.Magnitude);

        public static bool operator <(NonNumericCost left, NonNumericCost right) => left.CompareTo(right) < 0;

        public static bool operator >(NonNumericCost left, NonNumericCost right) => left.CompareTo(right) > 0;

        public static bool operator <=(NonNumericCost left, NonNumericCost right) => left.CompareTo(right) <= 0;

        public static bool operator >=(NonNumericCost left, NonNumericCost right) => left.CompareTo(right) >= 0;

        public readonly int CompareTo(NonNumericCost other) => Magnitude.CompareTo(other.Magnitude);
    }
}

internal record SearchBehaviourTestCase(LinqGraph Graph, int SourceId, int TargetId, (int from, int to)[] ExpectedSteps)
{
    public override string ToString()
    {
        return $"{{ Graph: {string.Join(", ", Graph.Edges.Select(e => $"({e.From.Id}, {e.To.Id})"))}, Source: {SourceId}, Target: {TargetId} }}";
    }
}

internal record AsyncSearchBehaviourTestCase(AsyncLinqGraph Graph, int SourceId, int TargetId, (int from, int to)[] ExpectedSteps)
{
    public override string ToString()
    {
        return $"{{ Graph: {string.Join(", ", Graph.Edges.Select(e => $"({e.From.Id}, {e.To.Id})").ToListAsync().AsTask().GetAwaiter().GetResult())}, Source: {SourceId}, Target: {TargetId} }}";
    }
}
