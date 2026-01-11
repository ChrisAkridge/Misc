using Celarix.JustForFun.NutritionFactsGenerator.Models;
using Celarix.JustForFun.NutritionFactsGenerator.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Data
{
    internal static class NutritionFactsInputs
    {
        public static IReadOnlyList<IInputRow> Elements()
        {
            return
            [
                // Group 0: Serving and Daily Energy Intake Info
                new InputDescription("dailyEnergyIntake", "Daily Energy Intake", 0, "Cal")
                    .AddInputElement(InputType.Number),
                new InputDescription("servingSize", "Serving Size", 0)
                    .AddInputElement(InputType.Number)
                    .AddInputElement(new InputElement("servingSizeUnit", InputType.Radio)
                        .WithOptions("grams", "milliliters")),
                new InputDescription("unitsPerServing", "Units Per Serving", 0)
                    .AddInputElement(new InputElement("foodIsDiscrete", InputType.Checkbox)
                        .WithOptions("Food is discrete"))
                    .AddInputElement(InputType.Number),
                new InputDescription("containerNetWeight", "Container Net Weight", 0, "g or mL")
                    .AddInputElement(InputType.Number),
                new ComputedDescription("servingsPerContainer", "Servings Per Container", 0,
                    "inputs.containerNetWeight / inputs.servingSize",
                    "", unitIsComputed: false),
                // Group 1: Calories
                new InputDescription("calories", "Calories", 1).AddInputElement(InputType.Number),
                // Group 2: Fat
                new InputDescription("totalFat", "Total Fat", 2, "g").AddInputElement(InputType.Number),
                new InputDescription("saturatedFat", "Saturated Fat", 2, "g").AddInputElement(InputType.Number),
                new InputDescription("polyunsaturatedFat", "Polyunsaturated Fat", 2, "g").AddInputElement(InputType.Number),
                new InputDescription("transFat", "Trans Fat", 2, "g").AddInputElement(InputType.Number),
                new ComputedDescription("monounsaturatedFat", "Monounsaturated Fat", 2,
                    "inputs.totalFat - inputs.saturatedFat - inputs.polyunsaturatedFat - inputs.transFat", "g", unitIsComputed: false),
                // Group 3: Cholesterol and Sodium
                new InputDescription("cholesterol", "Cholesterol", 3, "mg").AddInputElement(InputType.Number),
                new InputDescription("sodium", "Sodium", 3, "mg").AddInputElement(InputType.Number),
                // Group 4: Carbohydrates
                new InputDescription("totalCarbohydrates", "Total Carbohydrates", 4, "g").AddInputElement(InputType.Number),
                new InputDescription("dietaryFibers", "Dietary Fibers", 4, "g").AddInputElement(InputType.Number),
                new InputDescription("totalSugars", "Total Sugars", 4, "g").AddInputElement(InputType.Number),
                new InputDescription("addedSugars", "Added Sugars", 4, "g").AddInputElement(InputType.Number),
                new ComputedDescription("naturalSugars", "Natural Sugars", 4,
                    "inputs.totalSugars - inputs.addedSugars", "g", unitIsComputed: false),
                // Group 5: Vitamins and Minerals
                new InputDescription("biotin", "Biotin", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("choline", "Choline", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("folate", "Folate/Folic Acid", 5, "μg DFE").AddInputElement(InputType.Number),
                new InputDescription("niacin", "Niacin", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("pantotheticAcid", "Pantothetic Acid", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("riboflavin", "Riboflavin", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("thiamin", "Thiamin", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("vitaminA", "Vitamin A", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("vitaminB6", "Vitamin B".WithSubscript("6"), 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("vitaminB12", "Vitamin B".WithSubscript("12"), 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("vitaminC", "Vitamin C", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("vitaminD", "Vitamin D", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("vitaminE", "Vitamin E", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("vitaminK", "Vitamin K", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("calcium", "Calcium", 5, "g").AddInputElement(InputType.Number),
                new InputDescription("chloride", "Chloride", 5, "g").AddInputElement(InputType.Number),
                new InputDescription("chromium", "Chromium", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("copper", "Copper", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("iodine", "Iodine", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("iron", "Iron", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("magnesium", "Magnesium", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("manganese", "Manganese", 5, "mg").AddInputElement(InputType.Number),
                new InputDescription("molybdenum", "Molybdenum", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("potassium", "Potassium", 5, "g").AddInputElement(InputType.Number),
                new InputDescription("selenium", "Selenium", 5, "μg").AddInputElement(InputType.Number),
                new InputDescription("zinc", "Zinc", 5, "mg").AddInputElement(InputType.Number),
            ];
        }

        public static string GenerateNutritionFactsJSClass(IEnumerable<IInputRow> inputRows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("class NutritionFactsInputs {");
            sb.AppendLine("    constructor() {");
            foreach (var row in inputRows)
            {
                if (row is InputDescription inputDesc)
                {
                    foreach (var element in inputDesc.InputElements)
                    {
                        sb.AppendLine($"        this.{element.Id} = null;");
                    }
                }
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenerateNutritionFactsBuilderJSFunction(IEnumerable<IInputRow> inputRows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("function buildNutritionFactsInputs() {");
            sb.AppendLine("    const inputs = new NutritionFactsInputs();");
            foreach (var row in inputRows)
            {
                if (row is InputDescription inputDesc)
                {
                    foreach (var element in inputDesc.InputElements)
                    {
                        sb.AppendLine($"    inputs.{element.Id} = {element.ToJsGetValue()};");
                    }
                }
            }
            sb.AppendLine("    return inputs;");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenerateOnChangeEventListeners(IEnumerable<IInputRow> inputRows)
        {
            var sb = new StringBuilder();
            foreach (var row in inputRows)
            {
                if (row is InputDescription inputDesc)
                {
                    foreach (var element in inputDesc.InputElements)
                    {
                        if (element.Type == InputType.Radio)
                        {
                            sb.AppendLine($"document.querySelectorAll('input[name=\"{element.Id}\"]').forEach(radio => {{");
                            sb.AppendLine("    radio.addEventListener('change', () => {");
                            sb.AppendLine("        const inputs = buildNutritionFactsInputs();");
                            sb.AppendLine("        onUpdate(inputs);");
                            sb.AppendLine("    });");
                            sb.AppendLine("});");
                            continue;
                        }

                        sb.AppendLine($"document.getElementById('{element.Id}').addEventListener('change', () => {{");
                        sb.AppendLine("    const inputs = buildNutritionFactsInputs();");
                        sb.AppendLine("    onUpdate(inputs);");
                        sb.AppendLine("});");
                    }
                }
            }
            return sb.ToString();
        }

        public static string GenerateOnUpdateJsStatements(IEnumerable<IInputRow> inputRows)
        {
            var sb = new StringBuilder();
            foreach (var row in inputRows)
            {
                if (row is ComputedDescription computedDesc)
                {
                    sb.AppendLine(computedDesc.OnUpdateJsStatements());
                }
            }
            return sb.ToString();
        }
    }
}
