using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public class PresentationManager
    {
        private readonly List<IPresenter> presenters;
        private readonly List<IAsyncPresenter> asyncPresenters;

        public PresentationManager(Panel panel, Settings settings)
        {
            var y = 5;
            presenters = new List<IPresenter>
            {
                new TimeVisualizerPresenter(panel, settings, y, out y),
                new TimeDisplayPresenter(panel, settings, y, out y),
                new RandomValuePresenter(panel, settings, y, out y),
                new StaticURLImagePresenter(panel, settings, y, out y),
                //new YahtzeePresenter(panel, settings.YahtzeePlayerUpdateTime, y, out y),
                new CountdownPresenter(panel, y, out y)
            };

            asyncPresenters = new List<IAsyncPresenter>
            {
                new StockQuotePresenter(panel, y, out y),
                new WeatherPresenter(panel, y, out y)
            };
            
            // Add new presenters down here
        }

        public void Render(int timerTicks)
        {
            foreach (var presenter in presenters)
            {
                presenter.Render(timerTicks);
            }
        }

        public async Task RenderAsync(int timerTicks)
        {
            foreach (var asyncPresenter in asyncPresenters)
            {
                await asyncPresenter.Render(timerTicks);
            }
        }
    }
}
