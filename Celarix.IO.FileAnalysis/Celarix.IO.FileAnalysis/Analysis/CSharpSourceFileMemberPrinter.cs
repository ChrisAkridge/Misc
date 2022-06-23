using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.Analysis
{
	internal static class CSharpSourceFileMemberPrinter
    {
        private const string MemberPath = "members.cs";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool TryPrintSourceFileMembers(string sourceFilePath)
        {
            logger.Trace($"Printing members of C# source file at {sourceFilePath}...");

            try
            {
                if (!IsCSharpSourceFile(sourceFilePath))
                {
                    return false;
                }
                
                var members = PrintSourceFileMembers(sourceFilePath);
                var savePath = GetSavePath(sourceFilePath);
                LongDirectory.CreateDirectory(LongPath.GetDirectoryName(savePath));
                LongFile.WriteAllText(savePath, members);

                return true;
            }
            catch
            {
                logger.Trace($"{sourceFilePath} was a text file but likely not a C# source file");

                return false;
            }
        }

        private static bool IsCSharpSourceFile(string sourceFilePath) =>
            sourceFilePath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)
            || sourceFilePath.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase);

        private static string GetSavePath(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + "_ext",
                MemberPath);

        private static string PrintSourceFileMembers(string sourceFilePath)
        {
            var code = LongFile.ReadAllText(sourceFilePath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var builder = new StringBuilder();

            foreach (var member in root.Members)
            {
                builder.AppendLine(PrintMember(member, 0));
            }

            return builder.ToString();
        }

        private static string PrintMember(MemberDeclarationSyntax member, int tabLevel)
        {
            return member switch
            {
                NamespaceDeclarationSyntax namespaceDeclaration => PrintNamespace(namespaceDeclaration, tabLevel),
                ClassDeclarationSyntax classDeclaration => PrintClass(classDeclaration, tabLevel),
                StructDeclarationSyntax structDeclaration => PrintStruct(structDeclaration, tabLevel),
                InterfaceDeclarationSyntax interfaceDeclaration => PrintInterface(interfaceDeclaration, tabLevel),
                EnumDeclarationSyntax enumDeclaration => PrintEnum(enumDeclaration, tabLevel),
                EnumMemberDeclarationSyntax enumMemberDeclaration => PrintEnumMember(enumMemberDeclaration, tabLevel),
                DelegateDeclarationSyntax delegateDeclaration => PrintDelegate(delegateDeclaration, tabLevel),
                FieldDeclarationSyntax fieldDeclaration => PrintField(fieldDeclaration, tabLevel),
                PropertyDeclarationSyntax propertyDeclaration => PrintProperty(propertyDeclaration, tabLevel),
                EventDeclarationSyntax eventDeclaration => PrintEventDeclaration(eventDeclaration, tabLevel),
                ConstructorDeclarationSyntax constructorDeclaration => PrintConstructor(constructorDeclaration,
                    tabLevel),
                DestructorDeclarationSyntax destructorDeclaration => PrintDestructor(destructorDeclaration, tabLevel),
                MethodDeclarationSyntax methodDeclaration => PrintMethod(methodDeclaration, tabLevel),
                _ => ""
            };
        }

        private static string PrintNamespace(NamespaceDeclarationSyntax namespaceDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var builder = new StringBuilder($"{tabs}namespace {namespaceDeclaration.Name.GetText().ToString().Trim()} {{\r\n");
            
            foreach (var member in namespaceDeclaration.Members)
            {
                builder.AppendLine(PrintMember(member, tabLevel + 1));
            }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private static string PrintClass(ClassDeclarationSyntax classDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", classDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var builder = new StringBuilder($"{tabs}{modifiers} class {classDeclaration.Identifier.Text.Trim()}{PrintGenericParameters(classDeclaration.Arity)} {{\r\n");

            foreach (var member in classDeclaration.Members)
            {
                builder.AppendLine(PrintMember(member, tabLevel + 1));
            }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private static string PrintStruct(StructDeclarationSyntax structDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", structDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var builder =
                new StringBuilder(
                    $"{tabs}{modifiers} struct {structDeclaration.Identifier.Text.Trim()}{PrintGenericParameters(structDeclaration.Arity)} {{\r\n");

            foreach (var member in structDeclaration.Members)
            {
                builder.AppendLine(PrintMember(member, tabLevel + 1));
            }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private static string PrintInterface(InterfaceDeclarationSyntax interfaceDefinition, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", interfaceDefinition.Modifiers.Select(m => m.Text.Trim()));
            var builder =
                new StringBuilder(
                    $"{tabs}{modifiers} interface {interfaceDefinition.Identifier.Text.Trim()}{PrintGenericParameters(interfaceDefinition.Arity)} {{\r\n");

            foreach (var member in interfaceDefinition.Members) { builder.AppendLine(PrintMember(member, tabLevel + 1)); }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private static string PrintEnum(EnumDeclarationSyntax enumDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", enumDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var builder =
                new StringBuilder(
                    $"{tabs}{modifiers} enum {enumDeclaration.Identifier.Text.Trim()} {{\r\n");

            foreach (var member in enumDeclaration.Members) { builder.AppendLine(PrintMember(member, tabLevel + 1)); }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private static string PrintEnumMember(EnumMemberDeclarationSyntax enumMemberDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            return $"{tabs}{enumMemberDeclaration.Identifier.Text.Trim()},";
        }

        private static string PrintDelegate(DelegateDeclarationSyntax delegateDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", delegateDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var returnType = PrintType(delegateDeclaration.ReturnType);
            var parameters = string.Join(", ", delegateDeclaration.ParameterList.Parameters
                .Select(p => $"{PrintType(p.Type)} {p.Identifier.Text.Trim()}"));

            return $"{tabs}{modifiers} delegate {returnType} {delegateDeclaration.Identifier.Text.Trim()}({parameters});";
        }

        private static string PrintField(FieldDeclarationSyntax fieldDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var builder = new StringBuilder();
            var modifiers = string.Join(" ", fieldDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var type = PrintType(fieldDeclaration.Declaration.Type);

            foreach (var variableDeclaration in fieldDeclaration.Declaration.Variables)
            {
                builder.AppendLine($"{tabs}{modifiers} {type} {variableDeclaration.Identifier.Text.Trim()};");
            }

            return builder.ToString().TrimEnd();
        }

        private static string PrintProperty(PropertyDeclarationSyntax propertyDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = string.Join(" ", propertyDeclaration.Modifiers.Select(m => m.Text.Trim()));
            var type = PrintType(propertyDeclaration.Type);
            var accessors = propertyDeclaration.AccessorList?.Accessors
                .Select(a => $"{JoinModifiers(a.Modifiers)} {a.Keyword.Text.Trim()};");
            var accessorString = (accessors != null)
                ? string.Join(" ", accessors)
                : "{}";

            return $"{tabs}{modifiers} {type} {propertyDeclaration.Identifier.Text.Trim()} {accessorString}";
        }

        private static string PrintEventDeclaration(EventDeclarationSyntax eventDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = JoinModifiers(eventDeclaration.Modifiers);
            var type = PrintType(eventDeclaration.Type);

            return $"{tabs}{modifiers} event {type} {eventDeclaration.Identifier.Text.Trim()};";
        }

        private static string PrintConstructor(ConstructorDeclarationSyntax constructorDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = JoinModifiers(constructorDeclaration.Modifiers);
            var parameters = string.Join(", ",
                constructorDeclaration.ParameterList.Parameters
                    .Select(p => $"{PrintType(p.Type)} {p.Identifier.Text.Trim()}"));

            return $"{tabs}{modifiers} {constructorDeclaration.Identifier.Text.Trim()}({parameters});";
        }

        private static string PrintDestructor(DestructorDeclarationSyntax destructorDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = JoinModifiers(destructorDeclaration.Modifiers);

            return $"{tabs}{modifiers} ~();";
        }

        private static string PrintMethod(MethodDeclarationSyntax methodDeclaration, int tabLevel)
        {
            var tabs = Tabs(tabLevel);
            var modifiers = JoinModifiers(methodDeclaration.Modifiers);
            var returnType = PrintType(methodDeclaration.ReturnType);
            var parameters = string.Join(", ",
                methodDeclaration.ParameterList.Parameters
                    .Select(p => $"{PrintType(p.Type)} {p.Identifier.Text.Trim()}"));

            return
                $"{tabs}{modifiers} {returnType} {methodDeclaration.Identifier.Text.Trim()}{PrintGenericParameters(methodDeclaration.Arity)}({parameters});";
        }

        private static string PrintType(TypeSyntax typeSyntax) => typeSyntax.GetText().ToString().Trim();

        private static string Tabs(int tabLevel) => new string('\t', tabLevel);

        private static string PrintGenericParameters(int arity)
        {
            return arity switch
            {
                0 => "",
                1 => "<T>",
                _ => "<" + string.Join(", ", Enumerable.Range(0, arity).Select(i => $"T{i}")) + ">"
            };
        }

		private static string JoinModifiers(SyntaxTokenList modifiers) => string.Join(" ", modifiers.Select(m => m.Text.Trim()));
	}
}
