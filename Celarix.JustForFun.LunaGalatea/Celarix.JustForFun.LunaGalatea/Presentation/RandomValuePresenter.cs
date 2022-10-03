using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public sealed class RandomValuePresenter : IPresenter
    {
        private readonly RandomValueProvider provider = new RandomValueProvider();
        private readonly Label valueLabel;

        public RandomValuePresenter(Panel panel, int startingY, out int endingY)
        {
            valueLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true
            };
            
            Render(0);
            startingY += TextRenderer.MeasureText(valueLabel.Text, valueLabel.Font).Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;

            valueLabel.Click += (_, _) => Render(0);
        }

        public void Render(int timerTicks)
        {
            // TODO: replace constant with value from settings
            if (timerTicks % 30 == 0)
            {
                valueLabel.Text = provider.GetDisplayObject();
            }
        }
    }
}
