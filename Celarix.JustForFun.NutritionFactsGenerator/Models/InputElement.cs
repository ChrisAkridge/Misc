using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed partial class InputElement(string id, InputType type)
    {
        private string[]? options;

        public string Id { get; } = id;
        public InputType Type { get; } = type;
        public IReadOnlyList<string>? Options => options;

        public InputElement WithOptions(params string[] options)
        {
            if (Type != InputType.Radio && Type != InputType.Checkbox)
            {
                throw new InvalidOperationException("Options can only be set for Radio or Checkbox input types.");
            }

            // Do ToArray to ensure a copy is made
            this.options = options.ToArray();
            return this;
        }

        public HtmlElement ToElement()
        {
            if (Type == InputType.Number)
            {
                return new HtmlElement("input")
                    .AddAttribute("type", "number")
                    .WithId(Id)
                    .AddAttribute("name", Id);
            }
            else if (Type == InputType.Radio)
            {
                var container = new HtmlElement("div");
                foreach (var option in options!)
                {
                    string optionId = FindWhitespace().Replace(option, "").CapitalizeFirstLetter();
                    var optionFullId = Id + optionId;
                    container.AddChild(new HtmlElement("input")
                        .WithId(optionFullId)
                        .AddAttribute("type", "radio")
                        .AddAttribute("name", Id)
                        .AddAttribute("value", optionId));
                    container.AddChild(new HtmlElement("label", option)
                        .AddAttribute("for", optionFullId));
                }
                return container;
            }
            else if (Type == InputType.Checkbox)
            {
                var checkboxText = options!.Single();
                var container = new HtmlElement("div");
                container.AddChild(new HtmlElement("input")
                    .WithId(Id)
                    .AddAttribute("type", "checkbox")
                    .AddAttribute("name", Id));
                container.AddChild(new HtmlElement("label", checkboxText)
                    .AddAttribute("for", Id));
                return container;
            }

            throw new InvalidOperationException($"Invalid input type {Type}");
        }

        public string ToJsGetValue()
        {
            if (Type == InputType.Number)
            {
                return $"parseFloat(document.getElementById('{Id}').value) || 0";
            }
            else if (Type == InputType.Radio)
            {
                return $"document.querySelector('input[name=\"{Id}\"]:checked').value;";
            }
            else if (Type == InputType.Checkbox)
            {
                return $"document.getElementById('{Id}').checked";
            }
            throw new InvalidOperationException($"Invalid input type {Type}");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex FindWhitespace();
    }
}
