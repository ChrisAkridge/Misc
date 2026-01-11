using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using Celarix.JustForFun.NutritionFactsGenerator.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed class InputDescription(string htmlElementId,
        string displayName,
        int groupNumber,
        string? unitName = null) : IInputRow
    {
        private readonly List<InputElement> inputElements = new();

        public string HtmlElementId { get; } = htmlElementId;
        public string DisplayName { get; } = displayName;
        public int GroupNumber { get; } = groupNumber;
        public string? UnitName { get; } = unitName;
        public IReadOnlyList<InputElement> InputElements => inputElements;
        
        public InputDescription AddInputElement(InputType inputType)
        {
            inputElements.Add(new InputElement(HtmlElementId, inputType));
            return this;
        }

        public InputDescription AddInputElement(InputElement inputElement)
        {
            inputElements.Add(inputElement);
            return this;
        }

        public HtmlElement ToElement()
        {
            var groupParity = $"grid-block-{GroupNumber % 2}";
            var fullDisplayName = DisplayName;
            if (UnitName != null)
            {
                fullDisplayName += $" ({UnitName})";
            }
            var tr = new HtmlElement("tr");
            tr.WithClass(groupParity);
            tr.AddChild(new HtmlElement("td")
                .AddChild(new HtmlElement("label", fullDisplayName)
                    .AddAttribute("for", HtmlElementId)));

            foreach (var inputElement in InputElements)
            {
                tr.AddChild(new HtmlElement("td")
                    .AddChild(inputElement.ToElement()));
            }

            while (tr.Children.Count < 3)
            {
                tr.AddChild(new HtmlElement("td"));
            }

            return tr;
        }
    }
}
