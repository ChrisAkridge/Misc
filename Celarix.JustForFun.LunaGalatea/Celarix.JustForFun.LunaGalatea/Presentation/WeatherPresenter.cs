using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Providers;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public class WeatherPresenter : IAsyncPresenter
    {
        private readonly WeatherProvider provider = new WeatherProvider();
        private readonly Label weatherLabel;

        public WeatherPresenter(Panel panel, int startingY, out int endingY)
        {
            weatherLabel = new Label
            {
                Location = new Point(5, startingY),
                Size = new Size(512, 250),
                Font = IPresenter.GetDisplayFont(provider.UseMonospaceFont)
            };

            startingY += weatherLabel.Height;

            var separatorLabel = new Label
            {
                AutoSize = false,
                Location = new Point(5, startingY),
                Size = new Size(panel.Width - 10, 2),
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            panel.Controls.Add(weatherLabel);
            panel.Controls.Add(separatorLabel);

            endingY = startingY + separatorLabel.Height + 5;
        }

        public async Task Render(int timerTicks)
        {
            try
            {
                var result = await provider.TryGetWeatherResponse(timerTicks);
                if (result.Success)
                {
                    weatherLabel.Text = result.DisplayObject;
                }
            }
            catch (Exception ex)
            {
                weatherLabel.Text =
                    $"Loading weather data failed with exception {ex.GetType().Name}:{Environment.NewLine}{ex.Message}";
            }
        }
    }
}
