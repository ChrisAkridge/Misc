using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;
using Label = System.Windows.Forms.Label;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public sealed class TimeVisualizerPresenter : IPresenter
    {
        private readonly TimeVisualizerProvider provider = new TimeVisualizerProvider();
        private readonly Label timeLabel;

        public TimeVisualizerPresenter(Panel panel, Settings settings, int startingY, out int endingY)
        {
            timeLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true,
                Font = IPresenter.GetDisplayFont(provider.UseMonospaceFont)
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
