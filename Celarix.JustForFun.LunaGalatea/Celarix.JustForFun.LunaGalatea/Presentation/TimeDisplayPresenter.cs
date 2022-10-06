using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public sealed class TimeDisplayPresenter : IPresenter
    {
        private readonly TimeDisplayProvider provider = new TimeDisplayProvider();
        private readonly Settings settings;
        
        private readonly Label timeLabel;

        public TimeDisplayPresenter(Panel panel, Settings settings, int startingY, out int endingY)
        {
            this.settings = settings;
            
            timeLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true
            };

            Render(0);
            startingY += TextRenderer.MeasureText(timeLabel.Text, timeLabel.Font).Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(timeLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public void Render(int timerTicks)
        {
            var timeDisplay = provider.GetDisplayObject();
            timeLabel.Text = string.Join(Environment.NewLine, timeDisplay);
        }
    }
}
