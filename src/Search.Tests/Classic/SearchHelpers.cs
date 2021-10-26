using SCGraphTheory.Search.TestGraphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCGraphTheory.Search.Classic
{
    internal static class SearchHelpers
    {
        public static (int from, int to)[] GetStepsToCompletion(ISearch<LinqGraph.Node, LinqGraph.Edge> search)
        {
            var searchSteps = new List<(int, int)>();
            while (!search.IsConcluded)
            {
                search.NextStep();

                var exploredEdges = search.Visited.Values
                    .Where(ke => !ke.IsOnFrontier && ke.Edge != null)
                    .Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id));

                var addedExploredEdges = exploredEdges.Except(searchSteps);
                var removedExploredEdges = searchSteps.Except(exploredEdges);

                if (addedExploredEdges.Count() != 1 || removedExploredEdges.Count() != 0)
                {
                    throw new Exception("Search explored no or multiple edges in a step, or forgot an explored edge");
                }

                searchSteps.Add(addedExploredEdges.Single());
            }

            return searchSteps.ToArray();
        }

        public static (int from, int to)[][] GetIterativeDeepeningStepsToCompletion(ISearch<LinqGraph.Node, LinqGraph.Edge> search)
        {
            var searchSteps = new List<(int, int)[]>();
            var currentIteration = new List<(int, int)>();
            while (!search.IsConcluded)
            {
                search.NextStep();

                var exploredEdges = search.Visited.Values
                    .Where(ke => !ke.IsOnFrontier && ke.Edge != null)
                    .Select(ke => (ke.Edge.From.Id, ke.Edge.To.Id));

                var addedExploredEdges = exploredEdges.Except(currentIteration);
                var removedExploredEdges = currentIteration.Except(exploredEdges);

                if (addedExploredEdges.Count() == 1 && removedExploredEdges.Count() == 0)
                {
                    // Same iteration
                    currentIteration.Add(addedExploredEdges.Single());
                }
                else if (addedExploredEdges.Count() == 0 && removedExploredEdges.Count() == currentIteration.Count() - 1)
                {
                    // New iteration
                    searchSteps.Add(currentIteration.ToArray());
                    currentIteration = new List<(int, int)>();
                    currentIteration.Add(exploredEdges.Single());
                }
                else
                {
                    throw new Exception("Search did something unexpected");
                }
            }

            searchSteps.Add(currentIteration.ToArray());

            return searchSteps.ToArray();
        }
    }
}
