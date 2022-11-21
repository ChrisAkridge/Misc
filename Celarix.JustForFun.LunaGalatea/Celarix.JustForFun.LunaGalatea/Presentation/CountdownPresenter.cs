using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public class CountdownPresenter : IPresenter
    {
        private readonly CountdownProvider provider = new CountdownProvider(SystemClock.Instance);
        private readonly Label countdownLabel;

        public CountdownPresenter(Panel panel, Settings settings, int startingY, out int endingY)
        {
            countdownLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true
            };
            
            Render(0);
            startingY += TextRenderer.MeasureText(countdownLabel.Text, countdownLabel.Font).Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(countdownLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public void Render(int timerTicks)
        {
            var countdownDisplay = provider.GetDisplayObject();
            countdownLabel.Text = string.Join(Environment.NewLine, countdownDisplay);
        }
    }
}
