using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Presentation
{
    public interface IPresenter
    {
        void Render(int timerTicks);

        static Font GetDisplayFont(bool useMonospaceFont) => useMonospaceFont
            ? new Font("Consolas", 9.25f)
            : new Font("Segoe UI", 9.25f);
    }
}
