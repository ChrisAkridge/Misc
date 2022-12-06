using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public interface IAsyncPresenter
    {
        Task Render(int timerTicks);
    }
}
