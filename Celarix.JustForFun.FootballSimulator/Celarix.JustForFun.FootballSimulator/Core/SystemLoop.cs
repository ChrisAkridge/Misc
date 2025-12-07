using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    public sealed partial class SystemLoop
    {
        private Random random;
        private FootballContext footballContext;
        private GameLoop? currentGameLoop;

        public CurrentSystemState CurrentState { get; private set; }
        public SystemStatus SystemStatus { get; private set; }

        public SystemLoop(Random random)
        {
            footballContext = new FootballContext();
            CurrentState = CurrentSystemState.Initialization;
            SystemStatus = new SystemStatus
            {
                CurrentState = CurrentState,
                StatusMessage = "Football Simulator started."
            };
            this.random = random;
        }

        public void MoveNext()
        {
            try
            {
                switch (CurrentState)
                {
                    case CurrentSystemState.Initialization:
                        Initialize();
                        break;
                    case CurrentSystemState.GameInitialization:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during system loop execution in state {State}.", CurrentState);
                SystemStatus = new SystemStatus
                {
                    CurrentState = CurrentState,
                    StatusMessage = $"An error occurred: {ex.Message}"
                };
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            switch (CurrentState)
            {
                case CurrentSystemState.Initialization:
                    // Hard to recover from this state, leave it as-is.
                    Log.Fatal("Fatal error during initialization. Stopping the simulator.");
                    CurrentState = CurrentSystemState.Faulted;
                    break;
            }
        }
    }
}
