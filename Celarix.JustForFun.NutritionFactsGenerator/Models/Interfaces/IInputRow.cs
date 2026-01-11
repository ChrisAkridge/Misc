using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models.Interfaces
{
    internal interface IInputRow
    {
        HtmlElement ToElement();
    }
}
