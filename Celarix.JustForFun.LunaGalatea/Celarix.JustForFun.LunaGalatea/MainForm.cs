using Celarix.JustForFun.LunaGalatea.Presentation;

namespace Celarix.JustForFun.LunaGalatea
{
    public partial class MainForm : Form
    {
        private readonly PresentationManager presentationManager;
        private int timerTicks;
        
        public MainForm()
        {
            InitializeComponent();

            presentationManager = new PresentationManager(MainPanel);
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            timerTicks += 1;
            presentationManager.Render(timerTicks);
        }
    }
}