using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpToCixCore
{
	class Program
	{
		private static async Task Main(string[] args)
        {
            var solutionPath = args[0];
            var outputPath = args[1];

            var manager = new AnalyzerManager(solutionPath);
            var outputText = new List<string>();

            using (var workspace = manager.GetWorkspace())
            {
                foreach (var project in workspace.CurrentSolution.Projects.OrderBy(p => p.Name))
                {
                    var compilation = (CSharpCompilation)await project.GetCompilationAsync();

                    foreach (var syntaxTree in compilation.SyntaxTrees)
                    {
                        Console.WriteLine($"Processing {syntaxTree.FilePath}...");

                        var model = compilation.GetSemanticModel(syntaxTree);
                        var tree = new TreeBuilder();
                        tree.BuildTree(syntaxTree, model);

                        var output = new TreeWriter().WriteTree(tree);
                        outputText.Add(output);
                    }
                }
            }

            File.WriteAllText(outputPath, string.Join("\r\n", outputText));
        }
	}
}
