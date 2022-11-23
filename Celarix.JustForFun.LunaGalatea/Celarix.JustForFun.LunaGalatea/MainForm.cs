using Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;
using Celarix.JustForFun.LunaGalatea.Presentation;

namespace Celarix.JustForFun.LunaGalatea
{
    public partial class MainForm : Form
    {
        private readonly PresentationManager presentationManager;
        private int timerTicks;
        private Settings settings;
        
        public MainForm()
        {
            InitializeComponent();

            settings = Settings.LoadOrCreate();
            YahtzeePlayer.GameOver += YahtzeePlayer_GameOver;
            
            presentationManager = new PresentationManager(MainPanel, settings);
        }

        private void YahtzeePlayer_GameOver(object? sender, YahtzeeInfo e)
        {
            settings.YahtzeeInfo.TotalGamesPlayed += 1;
            settings.YahtzeeInfo.TotalPointsScored += e.TotalGamesPlayed;
            settings.YahtzeeInfo.TotalPointsScored += e.TotalPointsScored;
            settings.YahtzeeInfo.TotalYahtzeeCount += e.TotalYahtzeeCount;
            settings.YahtzeeInfo.AveragePointsPerGame =
                (double)settings.YahtzeeInfo.TotalPointsScored / settings.YahtzeeInfo.TotalGamesPlayed;
            settings.YahtzeeInfo.HighScore = Math.Max(settings.YahtzeeInfo.HighScore, (int)e.TotalPointsScored);
            
            settings.Save();
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            timerTicks += 1;
            presentationManager.Render(timerTicks);
        }
    }
}