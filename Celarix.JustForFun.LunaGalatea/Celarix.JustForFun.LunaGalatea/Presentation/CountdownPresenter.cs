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
        private const int PageSize = 10;
        
        private readonly CountdownProvider provider = new CountdownProvider(SystemClock.Instance);
        private readonly Label countdownLabel;
        private readonly string[] currentPage = new string[PageSize];
        private int currentPageIndex = 0;
        private int rendersSincePageChange = 0;

        public CountdownPresenter(Panel panel, int startingY, out int endingY)
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
            var countdowns = provider.GetDisplayObject();
            
            FillPage(countdowns);
            countdownLabel.Text = string.Join(Environment.NewLine, currentPage);
            rendersSincePageChange += 1;
            
            if (rendersSincePageChange % 10 == 0) { NextPage(countdowns); }
        }

        private void FillPage(IReadOnlyList<string> allCountdowns)
        {
            Array.Fill(currentPage, "");
            
            var pageOffset = currentPageIndex * PageSize;
            var countdownsOnPage = Math.Min(allCountdowns.Count - pageOffset, PageSize);

            for (var i = 0; i < countdownsOnPage; i++)
            {
                var countdownOffset = pageOffset + i;
                currentPage[i] = allCountdowns[countdownOffset];
            }
        }

        private void NextPage(IReadOnlyCollection<string> allCountdowns)
        {
            var nextPageOffset = (currentPageIndex + 1) * PageSize;
            if (nextPageOffset >= allCountdowns.Count)
            {
                currentPageIndex = 0;
            }
            else
            {
                currentPageIndex += 1;
            }
        }
    }
}
