using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    public sealed partial class SystemLoop
    {
        private SystemContext context;

        public SystemLoop()
        {
            var firstNames = File.ReadAllLines("Names\\player-first-names.csv");
            var lastNames = File.ReadAllLines("Names\\player-last-names.csv")
                .Select(Helpers.CapitalizeLastName);

            context = new SystemContext(
                Version: 0L,
                NextState: SystemState.Start,
                Environment: new SystemEnvironment
                {
                    FootballRepository = new FootballRepository(new FootballContext()),
                    RandomFactory = new RandomFactory(),
                    PlayerFactory = new PlayerFactory(firstNames, lastNames)
                });
        }

        public AdvancedStateMachine MoveNext()
        {
            InGameSignal inGameSignal = InGameSignal.None;

            context = context.NextState switch
            {
                SystemState.Start => StartStep.Run(context),
                SystemState.InitializeDatabase => InitializeDatabaseStep.Run(context),
                SystemState.PrepareForGame => PrepareForGameStep.Run(context),
                SystemState.InitializeNextSeason => InitializeNextSeasonStep.Run(context),
                SystemState.InitializeWildCardRound => InitializeWildCardRoundStep.Run(context),
                SystemState.InitializeDivisionalRound => InitializeDivisionalRoundStep.Run(context),
                SystemState.InitializeConferenceChampionshipRound => InitializeConferenceChampionshipRoundStep.Run(context),
                SystemState.InitializeSuperBowl => InitializeSuperBowlStep.Run(context),
                SystemState.ResumePartialGame => ResumePartialGameStep.Run(context),
                SystemState.LoadGame => LoadGameStep.Run(context),
                SystemState.InGame => InGameStep.Run(context, out inGameSignal),
                SystemState.PostGame => PostGameStep.Run(context),
                SystemState.WriteSummaryForGame => WriteSummaryForGameStep.Run(context),
                SystemState.WriteSummaryForSeason => WriteSummaryForSeasonStep.Run(context),
                SystemState.Error => ErrorStep.Run(context),
                _ => throw new ArgumentOutOfRangeException($"Unhandled system state: {context.NextState}")
            };

            if (context.NextState == SystemState.InGame)
            {
                return inGameSignal switch
                {
                    InGameSignal.None => AdvancedStateMachine.System,
                    InGameSignal.PlayEvaluationStep => AdvancedStateMachine.PlayEvaluation,
                    InGameSignal.GameStateAdvanced => AdvancedStateMachine.Game,
                    InGameSignal.GameCompleted => AdvancedStateMachine.System,
                    _ => throw new ArgumentOutOfRangeException($"InGameStep returned unexpected signal: {inGameSignal}")
                };
            }
        }
    }
}
