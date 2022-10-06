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
            presentationManager = new PresentationManager(MainPanel, settings);
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            timerTicks += 1;
            presentationManager.Render(timerTicks);
        }
    }
}