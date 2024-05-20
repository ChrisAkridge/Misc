using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public class YahtzeePresenter : IPresenter
    {
        private readonly Label label;
        private readonly int updateTime;

        public YahtzeePresenter(Panel panel, int updateTime, int startingY, out int endingY)
        {
            this.updateTime = updateTime;
            YahtzeePlayer.NextStep();
            
            label = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 10, 0),
                Text = YahtzeePlayer.GetDisplayText(),
                Font = IPresenter.GetDisplayFont(false)
            };

            startingY += TextRenderer.MeasureText(label.Text, label.Font).Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(label);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public void Render(int timerTicks)
        {
            //if (timerTicks % updateTime == 0)
            //{
                YahtzeePlayer.NextStep();
                label.Text = YahtzeePlayer.GetDisplayText();
            //}
        }
    }
}
