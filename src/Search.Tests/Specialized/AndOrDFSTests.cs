﻿using FluentAssertions;
using FlUnit;
using SCGraphTheory.Search.Benchmarks.AlternativeSearches.Specialized;
using System.Collections.Generic;
using System.Linq;
using static SCGraphTheory.Search.TestGraphs.SpecificScenarios.AiAModernApproach.ErraticVacuumWorldGraph;

namespace SCGraphTheory.Search.Specialized
{
    public static class AndOrDFSTests
    {
        public static Test ErraticVaccumWorld_FromAIaMA => TestThat
            .When(() =>
            {
                var initialState = new State(
                    VacuumPosition: VacuumPositions.Left,
                    IsCurrentLocationDirty: true,
                    IsOtherLocationDirty: true);

                return AndOrDFS_FromAIaMA.Execute<INode, IEdge>(
                    GetStateNode(initialState),
                    n => !n.State.IsLeftDirty && !n.State.IsRightDirty);
            })
            .ThenReturns()
            .And((o) => o.Succeeded.Should().BeTrue())
            .And((o) => o.Plan.Flatten().ToDictionary(kvp => kvp.Key.State, kvp => kvp.Value.Action).Should().BeEquivalentTo(new Dictionary<State, Actions>
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