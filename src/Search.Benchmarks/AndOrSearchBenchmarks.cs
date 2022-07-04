#pragma warning disable SA1600 // Elements should be documented
using BenchmarkDotNet.Attributes;
using SCGraphTheory.Search.AndOr;
using SCGraphTheory.Search.Benchmarks.AlternativeSearches.AndOr;
using SCGraphTheory.Search.TestGraphs.Specialized.AndOr;
using System;
using System.Linq;

namespace SCGraphTheory.Search.Benchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    public class AndOrSearchBenchmarks
    {
        private readonly PropositionalLogicGraph propositionalLogicGraph;
        private readonly string[] knownTruths;

        private readonly ErraticVacuumWorldGraph.State initialState;

        public AndOrSearchBenchmarks()
        {
            propositionalLogicGraph = new PropositionalLogicGraph(
                new PropositionalLogicGraph.DefiniteClause[]
                {
                    new (new[] { "Q", "R" }, "P"), // P if Q and R
                    new (new[] { "S" }, "P"), // P if S
                    new (new[] { "T" }, "Q"), // Q if T
                    new (new[] { "U" }, "Q"), // Q if U
                },
                false);

            knownTruths = new[] { "U", "R" };

            initialState = new ErraticVacuumWorldGraph.State(
                VacuumPosition: ErraticVacuumWorldGraph.VacuumPositions.Left,
                IsCurrentLocationDirty: true,
                IsOtherLocationDirty: true);
        }

        [Benchmark]
        [BenchmarkCategory(nameof(AndOrDFS<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>), nameof(ErraticVacuumWorldGraph))]
        public AndOrDFS<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>.Outcome AndOrDFS_ErraticVacuumWorld() => new AndOrDFS<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>(
            source: ErraticVacuumWorldGraph.GetStateNode(initialState),
            isTarget: n => !n.State.IsLeftDirty && !n.State.IsRightDirty,
            e => e is ErraticVacuumWorldGraph.ActionEdge).Execute();

        [Benchmark]
        [BenchmarkCategory(nameof(AndOrDFS_FromAIaMA), nameof(ErraticVacuumWorldGraph))]
        public AndOrDFS_FromAIaMA.Outcome<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge> AltAndOrDFS_ErraticVacuumWorld() => AndOrDFS_FromAIaMA.Execute<ErraticVacuumWorldGraph.INode, ErraticVacuumWorldGraph.IEdge>(
            source: ErraticVacuumWorldGraph.GetStateNode(initialState),
            isTarget: n => !n.State.IsLeftDirty && !n.State.IsRightDirty);

        [Benchmark]
        [BenchmarkCategory(nameof(AndOrDFS<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>), nameof(ErraticVacuumWorldGraph))]
        public AndOrDFS<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>.Outcome AndOrDFS_PLGraph() => new AndOrDFS<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>(
            source: propositionalLogicGraph.GetPropositionNode("P"),
            isTarget: n => knownTruths.Contains(n.Symbol),
            e => e is PropositionalLogicGraph.ClauseEdge).Execute();

        [Benchmark]
        [BenchmarkCategory(nameof(AndOrDFS_FromAIaMA), nameof(PropositionalLogicGraph))]
        public AndOrDFS_FromAIaMA.Outcome<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge> AltAndOrDFS_PLGraph() => AndOrDFS_FromAIaMA.Execute<PropositionalLogicGraph.INode, PropositionalLogicGraph.IEdge>(
            source: propositionalLogicGraph.GetPropositionNode("P"),
            isTarget: n => knownTruths.Contains(n.Symbol));
    }
}
#pragma warning restore SA1600