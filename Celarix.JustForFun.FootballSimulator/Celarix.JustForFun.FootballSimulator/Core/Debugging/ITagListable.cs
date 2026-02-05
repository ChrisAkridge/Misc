using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Debugging
{
    public interface ITagListable
    {
        IEnumerable<string> GetTags();
    }
}
