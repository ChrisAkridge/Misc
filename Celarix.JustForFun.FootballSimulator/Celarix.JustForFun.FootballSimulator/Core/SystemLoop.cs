using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Output.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    public sealed partial class SystemLoop
    {
        private SystemContext context;

        public SystemLoop(IEventBus eventBus)
        {
            var firstNames = File.ReadAllLines("Names\\player-first-names.csv");
            var lastNames = File.ReadAllLines("Names\\player-last-names.csv")
                .Select(Helpers.CapitalizeLastName);

            FootballRepository footballRepository = new(new FootballContext());
            var settings = InitializeSimulatorSettings(footballRepository);
            context = new SystemContext(
                Version: 0L,
                NextState: SystemState.Start,
                Environment: new SystemEnvironment
                {
                    FootballRepository = footballRepository,
                    RandomFactory = new RandomFactory(),
                    PlayerFactory = new PlayerFactory(firstNames, lastNames),
                    SummaryWriter = new OpenAISummaryWriter(),
                    DebugContextWriter = new DebugContextWriter(settings.SaveStateMachineContextsForDebugging, settings.StateMachineContextSavePath),
                    EventBus = eventBus
                });
            PublishGameEvent();
        }

        private static SimulatorSettings InitializeSimulatorSettings(IFootballRepository repository)
        {
            // We do need the simulator settings to at least exist before we can initialize the system
            // context, so let's do that here. InitializeDatabaseStep will still handle the rest of
            // the setup.
            var settings = repository.GetSimulatorSettings();

            if (settings == null)
            {
                settings = new()
                {
                    SeedDataInitialized = false,
                    StateMachineContextSavePath = "",
                    SaveStateMachineContextsForDebugging = false
                };
                repository.AddSimulatorSettings(settings);
                repository.SaveChanges();
            }

            return settings;
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

            context.Environment.DebugContextWriter.WriteContext(context, context.Environment);
            PublishGameEvent();

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
            return AdvancedStateMachine.System;
        }

        private void PublishGameEvent()
        {
            // This is the main place where game events are published.
            // It's very clunky, but we basically want to collect tags instead of having every level
            // of every state machine want to publish its own events. A new event is published every
            // step of the system state machine, which might contain a step of the game state machine
            // or the play evaluation state machine. Events aren't required to produce visible updates
            // in event listeners every time.
            var eventBus = context.Environment.EventBus;
            var gameEvent = new GameEvent
            {
                EventType = GameEventTypes.SystemStateMachineStep,
                SystemContext = context
            }.WithTags(context.CollectTags());
            eventBus.Publish(gameEvent);
        }
    }
}
