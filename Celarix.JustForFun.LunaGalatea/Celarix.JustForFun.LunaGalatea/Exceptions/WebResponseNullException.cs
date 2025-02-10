using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Exceptions
{
    public sealed class WebResponseNullException : Exception
    {
        public WebResponseNullException(string message) : base(message) { }
    }
}
