using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator
{
    internal static class TemplateSubstitution
    {
        public static string SubstituteTemplatePlaceholders(string template, Dictionary<string, string> substitutions)
        {
            var result = new StringBuilder(template);
            foreach (var substitution in substitutions)
            {
                result.Replace($"{{{{{substitution.Key}}}}}", substitution.Value);
            }
            return result.ToString();
        }
    }
}
