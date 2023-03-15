using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator
{
    internal sealed class GameLoop
    {
        private readonly FootballContext context;
        private readonly GameRecord currentGame;
        
        public string StatusMessage { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public GameLoop(FootballContext context, GameRecord currentGame)
        {
            this.context = context;
            this.currentGame = currentGame;
        }

        public void RunNextAction()
        {
            
        }
    }
}
