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

        public PresentationManager(Panel panel)
        {
            var y = 5;
            presenters = new List<IPresenter>
            {
                new TimeDisplayPresenter(panel, y, out y),
                new RandomValuePresenter(panel, y, out y),
                new StaticURLImagePresenter(panel, y, out y)
            };
        }

        public void Render(int timerTicks)
        {
            foreach (var presenter in presenters)
            {
                presenter.Render(timerTicks);
            }
        }
    }
}
