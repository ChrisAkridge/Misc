using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public interface IProvider<out TDisplay>
    {
        bool UseMonospaceFont { get; }
        
        TDisplay GetDisplayObject();
    }
}
