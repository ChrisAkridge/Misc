using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.Analysis.Clients
{
    internal static class ILSpyClient
    {
        private const string DecompilerFolderSuffix = "_dcmp";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly DecompilerSettings settings = new DecompilerSettings(LanguageVersion.Latest)
        {
            AggressiveInlining = false,
            AggressiveScalarReplacementOfAggregates = false,
            AlwaysCastTargetsOfExplicitInterfaceImplementationCalls = true,
            AlwaysQualifyMemberReferences = false,
            AlwaysShowEnumMemberValues = false,
            AlwaysUseBraces = true,
            AnonymousMethods = true,
            AnonymousTypes = true,
            ApplyWindowsRuntimeProjections = false,
            ArrayInitializers = true,
            AssumeArrayLengthFitsIntoInt32 = false,
            AsyncAwait = true,
            AsyncEnumerator = true,
            AsyncUsingAndForEachStatement = true,
            AutomaticEvents = true,
            AutomaticProperties = true,
            AwaitInCatchFinally = true,
            DecimalConstants = true,
            DecompileMemberBodies = true,
            Deconstruction = true,
            DictionaryInitializers = true,
            Discards = true,
            DoWhileStatement = true,
            Dynamic = true,
            ExpandMemberDefinitions = true,
            ExpandUsingDeclarations = true,
            ExpressionTrees = true,
            ExtensionMethods = true,
            ExtensionMethodsInCollectionInitializers = true,
            FixedBuffers = true,
            FoldBraces = false,
            ForEachStatement = true,
            ForEachWithGetEnumeratorExtension = true,
            ForStatement = true,
            FunctionPointers = true,
            GetterOnlyAutomaticProperties = true,
            InitAccessors = true,
            IntroduceIncrementAndDecrement = true,
            IntroduceReadonlyAndInModifiers = true,
            IntroduceRefModifiersOnStructs = true,
            IntroduceUnmanagedConstraint = true,
            LiftNullables = true,
            LoadInMemory = true,
            LocalFunctions = true,
            LockStatement = true,
            MakeAssignmentExpressions = true,
            NamedArguments = false,
            NativeIntegers = true,
            NonTrailingNamedArguments = true,
            NullableReferenceTypes = true,
            NullPropagation = true,
            ObjectOrCollectionInitializers = true,
            OptionalArguments = true,
            OutVariables = true,
            PatternBasedFixedStatement = true,
            QueryExpressions = true,
            Ranges = true,
            ReadOnlyMethods = true,
            RecordClasses = true,
            RefExtensionMethods = true,
            RemoveDeadCode = false,
            RemoveDeadStores = false,
            SeparateLocalVariableDeclarations = false,
            ShowDebugInfo = false,
            ShowXmlDocumentation = true,
            SparseIntegerSwitch = false,
            StackAllocInitializers = true,
            StaticLocalFunctions = true,
            StringConcat = true,
            StringInterpolation = true,
            SwitchExpressions = true,
            SwitchStatementOnString = true,
            ThrowExpressions = true,
            ThrowOnAssemblyResolveErrors = false,
            TupleComparisons = true,
            TupleConversions = true,
            TupleTypes = true,
            UseDebugSymbols = false,
            UseEnhancedUsing = true,
            UseExpressionBodyForCalculatedGetterOnlyProperties = true,
            UseImplicitMethodGroupConversion = true,
            UseLambdaSyntax = true,
            UsePrimaryConstructorSyntax = true,
            UseRefLocalsForAccurateOrderOfEvaluation = true,
            UseSdkStyleProjectFormat = true,
            UsingDeclarations = true,
            UsingStatement = true,
            WithExpressions = true,
            YieldReturn = true
        };

        public static bool TryDecompile(string filePath)
        {
            logger.Trace($"Attempting to decompile {filePath} as a managed assembly...");

            try
            {
                // Based on https://github.com/icsharpcode/ILSpy/blob/8eafbb3d901938c58c14b8cee258bf1c1dd8255f/ICSharpCode.Decompiler.Console/IlspyCmdProgram.cs#L185
                var outputFolderPath = GetOutputFolderPath(filePath);
                new LongDirectoryInfo(outputFolderPath).Create();

                if (LongDirectory.EnumerateFiles(outputFolderPath, "*", SearchOption.TopDirectoryOnly)
                    .Any(f => f.Contains("csproj")))
                {
                    logger.Warn($"The assembly at {filePath} has already been decompiled! Skipping...");
                    return true;
                }

                var module = new PEFile(filePath);
                var resolver = new UniversalAssemblyResolver(filePath, throwOnError: false, module.Reader.DetectTargetFrameworkId());

                var decompiler = new WholeProjectDecompiler(settings, resolver, resolver, null);

                // https://stackoverflow.com/a/13513854/2709212
                var decompilationTask = Task.Run(() => decompiler.DecompileProject(module, outputFolderPath));
                return decompilationTask.Wait(TimeSpan.FromMinutes(10d));
            }
            catch (Exception ex)
            {
                logger.LogException(ex);

                return false;
            }
        }

        public static string GetOutputFolderPath(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + DecompilerFolderSuffix);
    }
}
