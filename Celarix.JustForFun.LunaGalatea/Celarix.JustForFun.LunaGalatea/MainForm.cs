using Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;
using Celarix.JustForFun.LunaGalatea.Presentation;

namespace Celarix.JustForFun.LunaGalatea
{
    public partial class MainForm : Form
    {
        private readonly PresentationManager presentationManager;
        private int timerTicks;
        private readonly Settings settings;
        private readonly Random random = new Random();

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

        private async void TimerAsync_Tick(object sender, EventArgs e)
        {
            await presentationManager.RenderAsync(timerTicks);
        }

        private void NUDRNGMin_ValueChanged(object sender, EventArgs e)
        {
            NUDRNGMax.Minimum = NUDRNGMin.Value + 1;
        }

        private void NUDRNGMax_ValueChanged(object sender, EventArgs e)
        {
            NUDRNGMin.Maximum = NUDRNGMax.Value - 1;
        }

        private void ButtonRNGRoll_Click(object sender, EventArgs e)
        {
            var randomNumber = random.Next((int)NUDRNGMin.Value, (int)(NUDRNGMax.Value + 1));
            LabelRNGResult.Text = $"Result: {randomNumber}";
        }

        private void ButtonRollRandomDouble_Click(object sender, EventArgs e)
        {
            var randomDouble = random.NextDouble();
            LabelRandomDouble.Text = $"Random Double: {randomDouble:F3}";
            var oneInValue = 1m / NUDOneIn.Value;
            
            LabelRandomDouble.ForeColor = randomDouble <= (double)oneInValue ? Color.ForestGreen : Color.Black;
        }

        private void NUDOneIn_ValueChanged(object sender, EventArgs e)
        {
            LabelQuotient.Text = $"= {(1m / NUDOneIn.Value):F3}";
        }
    }
}