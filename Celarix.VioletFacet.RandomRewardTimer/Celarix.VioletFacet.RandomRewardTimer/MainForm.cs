using System.Media;

namespace Celarix.VioletFacet.RandomRewardTimer
{
    public partial class MainForm : Form
    {
        private readonly SoundPlayer player = new SoundPlayer("nsmbwii1up1.wav");
        private readonly Random random = new Random();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TimerPlaySound.Interval = random.Next((int)NUDMinimumDelay.Value, (int)NUDMaximumDelay.Value + 1) * 1000;
        }
        
        private void TimerPlaySound_Tick(object sender, EventArgs e)
        {
            player.Play();
            TimerPlaySound.Interval = random.Next((int)NUDMinimumDelay.Value, (int)NUDMaximumDelay.Value + 1) * 1000;
        }

        private void NUDMinimumDelay_ValueChanged(object sender, EventArgs e)
        {
            if (NUDMinimumDelay.Value + 1m > NUDMaximumDelay.Value)
            {
                NUDMaximumDelay.Value = NUDMinimumDelay.Value + 1m;
            }

            TimerPlaySound.Interval = random.Next((int)NUDMinimumDelay.Value, (int)NUDMaximumDelay.Value + 1) * 1000;
        }

        private void NUDMaximumDelay_ValueChanged(object sender, EventArgs e)
        {
            if (NUDMaximumDelay.Value - 1m < NUDMinimumDelay.Value)
            {
                NUDMinimumDelay.Value = NUDMaximumDelay.Value - 1m;
            }

            TimerPlaySound.Interval = random.Next((int)NUDMinimumDelay.Value, (int)NUDMaximumDelay.Value + 1) * 1000;
        }
    }
}