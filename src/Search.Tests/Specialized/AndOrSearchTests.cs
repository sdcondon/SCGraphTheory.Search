using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.Benchmarks.AlternativeSearches.Specialized;
using System.Collections.Generic;
using System.Linq;
using static SCGraphTheory.Search.TestGraphs.SpecificScenarios.AiAModernApproach.ErraticVacuumWorldGraph;

namespace SCGraphTheory.Search.Specialized
{
    public static class AndOrSearchTests
    {
        public static Test ErraticVaccumWorld_Recursive => TestThat
            .Given(() => new
            {
                InitialState = new State(
                    VacuumPosition: VacuumPositions.Left,
                    IsCurrentLocationDirty: true,
                    IsOtherLocationDirty: true),
            })
            .When(g =>
            {
                return AndOrSearch_FromAIaMA.Execute<INode, IEdge>(
                    GetStateNode(g.InitialState),
                    n => !n.State.IsLeftDirty && !n.State.IsRightDirty);
            })
            .ThenReturns()
            .And((_, o) => o.Succeeded.Should().BeTrue())
            .And((g, o) => o.Plan.Flatten(GetStateNode(g.InitialState)).ToDictionary(kvp => kvp.Key.State, kvp => kvp.Value.Action).Should().BeEquivalentTo(new Dictionary<State, Actions>
            {
                [new (VacuumPositions.Left, true, true)] = Actions.Suck,
                [new (VacuumPositions.Left, false, true)] = Actions.Right,
                [new (VacuumPositions.Right, true, false)] = Actions.Suck,
            }));

        ////public static Test ErraticVaccumWorld => TestThat
        ////    .Given(() =>
        ////    {
        ////        var initialState = new State(
        ////            VacuumPosition: VacuumPositions.Left,
        ////            IsCurrentLocationDirty: true,
        ////            IsOtherLocationDirty: true);

        ////        return new AndOrSearch<INode, IEdge>(
        ////            GetStateNode(initialState),
        ////            n => !n.State.IsLeftDirty && !n.State.IsRightDirty);
        ////    })
        ////    .When(g =>
        ////    {
        ////        while (!g.IsConcluded)
        ////        {
        ////            g.NextStep();
        ////        }
        ////    })
        ////    .ThenReturns()
        ////    .And(g =>
        ////    {
        ////        g.Plan.ToDictionary(kvp => kvp.Key.State, kvp => kvp.Value.Action).Should().BeEquivalentTo(new Dictionary<State, Actions>
        ////        {
        ////            [new (VacuumPositions.Left, true, true)] = Actions.Suck,
        ////            [new (VacuumPositions.Left, false, true)] = Actions.Right,
        ////            [new (VacuumPositions.Right, false, true)] = Actions.Suck,
        ////        });
        ////    });
    }
}
