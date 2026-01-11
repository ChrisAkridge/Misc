using Celarix.JustForFun.NutritionFactsGenerator;
using Celarix.JustForFun.NutritionFactsGenerator.Data;
using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using System.Text;

if (args.Length != 1)
{
    Console.WriteLine("Usage: NutritionFactsGenerator <output_directory>");
    return;
}
var outputDirectory = args[0];

var htmlTemplate = File.ReadAllText("Templates/nutrition_facts.html");
var cssTemplate = File.ReadAllText("Templates/nutrition_facts.css");
var javascriptTemplate = File.ReadAllText("Templates/nutrition_facts.js");

var javaScriptBuilder = new StringBuilder();
var onUpdateBuilder = new StringBuilder();

var rowDiv = new HtmlElement("div")
    .WithClass("row");

// Panel 1: Nutrition Facts Input
var inputs = NutritionFactsInputs.Elements();
var inputTable = InputTableGenerator.GenerateFromInputRows(
    inputs,
    "bg-primary-subtle",
    "Nutrition Facts Input"
);
rowDiv.AddChild(inputTable);

javaScriptBuilder.AppendLine(NutritionFactsInputs.GenerateNutritionFactsJSClass(inputs));
javaScriptBuilder.AppendLine(NutritionFactsInputs.GenerateNutritionFactsBuilderJSFunction(inputs));
javaScriptBuilder.AppendLine(NutritionFactsInputs.GenerateOnChangeEventListeners(inputs));
onUpdateBuilder.AppendLine(NutritionFactsInputs.GenerateOnUpdateJsStatements(inputs));

// Panel 2: Serving Size Info
var servingSizeInfo = ServingInfoMassData.Elements();
rowDiv.AddChild(servingSizeInfo.ToHtmlElement("bg-success-subtle"));
onUpdateBuilder.AppendLine(servingSizeInfo.GenerateOnUpdateJSStatements());

// Panel 3: Calorie Info
var calorieInfo = CalorieInfoData.Elements();
rowDiv.AddChild(calorieInfo.ToHtmlElement("bg-info-subtle"));
onUpdateBuilder.AppendLine(calorieInfo.GenerateOnUpdateJSStatements());

// Substitute generated strings into template
var substitutions = new Dictionary<string, string>
{
    { "Panels", rowDiv.ToHtmlString() },
    { "GeneratedJS", javaScriptBuilder.ToString() },
    { "OnUpdate", onUpdateBuilder.ToString() }
};

var finalHtml = TemplateSubstitution.SubstituteTemplatePlaceholders(htmlTemplate, substitutions);
var finalCss = TemplateSubstitution.SubstituteTemplatePlaceholders(cssTemplate, substitutions);
var finalJavascript = TemplateSubstitution.SubstituteTemplatePlaceholders(javascriptTemplate, substitutions);

File.WriteAllText(Path.Combine(outputDirectory, "nutrition_facts.html"), finalHtml);
File.WriteAllText(Path.Combine(outputDirectory, "nutrition_facts.css"), finalCss);
File.WriteAllText(Path.Combine(outputDirectory, "nutrition_facts.js"), finalJavascript);