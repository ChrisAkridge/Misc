﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public class StockQuotePresenter : IAsyncPresenter
    {
        private readonly StockQuoteProvider provider = new StockQuoteProvider();
        private readonly Label quoteLabel;

        public StockQuotePresenter(Panel panel, int startingY, out int endingY)
        {
            quoteLabel = new Label
            {
                Location = new Point(5, startingY),
                Size = new Size(512, 85),
                Font = IPresenter.GetDisplayFont(provider.UseMonospaceFont)
            };

            startingY += quoteLabel.Height;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            
            panel.Controls.Add(quoteLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public async Task Render(int timerTicks)
        {
            if (string.IsNullOrEmpty(quoteLabel.Text) || timerTicks % 1800 == 0)
            {
                if (MarketOpen())
                {
                    var quotes = await provider.GetDisplayObject();
                    quoteLabel.Text = string.Join(Environment.NewLine, quotes);
                }
            }
        }

        private static bool MarketOpen()
        {
            var now = DateTimeOffset.Now;
            return now.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
                && now.Hour switch
                {
                    < 9 => false,
                    9 => now.Minute >= 30,
                    _ => now.Hour < 16
                };
        }
    }
}
