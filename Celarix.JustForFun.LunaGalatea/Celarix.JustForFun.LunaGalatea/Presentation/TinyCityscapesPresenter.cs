using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public sealed class TinyCityscapesPresenter : IPresenter
    {
        private readonly TinyCityscapesProvider provider = new TinyCityscapesProvider();
        private readonly Label cityscapeLabel;

        public TinyCityscapesPresenter(Panel panel, Settings settings, int startingY, out int endingY)
        {
            cityscapeLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true
            };
            
            Render(0);
            startingY += TextRenderer.MeasureText(cityscapeLabel.Text, cityscapeLabel.Font).Height + 5;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            panel.Controls.Add(cityscapeLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public void Render(int timerTicks)
        {
            if (timerTicks % 30 == 0)
            {
                cityscapeLabel.Text = provider.GetDisplayObject();
            }
        }
    }
}
