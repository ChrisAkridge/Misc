using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public interface IAsyncProvider<TDisplay>
    {
        bool UseMonospaceFont { get; }
        Task<TDisplay> GetDisplayObject();
    }
}
