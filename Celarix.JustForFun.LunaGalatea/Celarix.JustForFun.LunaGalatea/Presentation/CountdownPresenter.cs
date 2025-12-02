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
        private readonly Button detailsButton;
        private readonly Label countdownLabel;
        private readonly string[] currentPage = new string[PageSize];
        private int currentPageIndex = 0;
        private int rendersSincePageChange = 0;
        private bool pausedOnCurrentPage = false;

        public CountdownPresenter(Panel panel, int startingY, out int endingY)
        {
            detailsButton = new Button
            {
                Text = "Details",
                Location = new Point(5, startingY),
                Size = new Size(75, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            startingY += detailsButton.Height + 5;

            countdownLabel = new Label
            {
                Location = new Point(5, startingY),
                AutoSize = true,
                Font = IPresenter.GetDisplayFont(provider.UseMonospaceFont)
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
            
            panel.Controls.Add(detailsButton);
            panel.Controls.Add(countdownLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;

            detailsButton.Click += (_, _) =>
            {
                var detailsForm = new CountdownDetailsForm();
                detailsForm.ShowDialog();
            };

            countdownLabel.Click += (_, e) =>
            {
                if (e is MouseEventArgs me)
                {
                    if (me.Button == MouseButtons.Left)
                    {
                        NextPage(provider.GetDisplayObject());
                        rendersSincePageChange = 0;
                    }
                    else if (me.Button == MouseButtons.Right)
                    {
                        pausedOnCurrentPage = !pausedOnCurrentPage;
                        rendersSincePageChange = 9;
                    }
                }
            };
        }

        public void Render(int timerTicks)
        {
            var countdowns = provider.GetDisplayObject();
            
            FillPage(countdowns);

            if (pausedOnCurrentPage)
            {
                currentPage[0] += "    PAUSE";
            }

            countdownLabel.Text = string.Join(Environment.NewLine, currentPage);
            rendersSincePageChange += 1;

            if (rendersSincePageChange % 10 == 0 && !pausedOnCurrentPage)
            {
                NextPage(countdowns);
            }
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
