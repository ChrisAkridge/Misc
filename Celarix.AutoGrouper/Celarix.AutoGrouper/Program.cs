using Celarix.AutoGrouper.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

if (args.Length != 1)
{
    Console.WriteLine("Usage: Celarix.AutoGrouper <path-to-file>");
    return;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine($"File not found: {args[0]}");
    return;
}

string filePath = args[0];
Console.WriteLine($"Processing file at: {filePath}");

#region Load Source File and Validate
static string PrependStandardHeader(string filePath)
{
    // Load the contents of the StandardHeader.txt bundled with this assembly
    string standardHeaderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "StandardHeader.txt");
    var standardHeader = File.ReadAllText(standardHeaderPath);
    var fileText = File.ReadAllText(filePath);
    return standardHeader + Environment.NewLine + fileText;
}

static SyntaxNode GetSyntaxRoot(string sourceCode)
{
    // Parse the source code into a SyntaxTree
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

    // Get the root node of the SyntaxTree
    SyntaxNode root = syntaxTree.GetRoot();
    return root;
}

static void ValidateSourceFileOrThrow(SyntaxNode root)
{
    // Step 1: Check for syntax errors in the source file
    var syntaxTree = root.SyntaxTree;
    var diagnostics = syntaxTree.GetDiagnostics();

    var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
    if (errors.Any())
    {
        throw new InvalidOperationException($"Source file contains syntax errors:\n{string.Join("\n", errors.Select(e => e.GetMessage()))}");
    }
}

static void TopLevelNodesAreClassesAndUsingDirectivesOrThrow(SyntaxNode root)
{
    // Get all top-level nodes (direct children of the root)
    var topLevelNodes = root.ChildNodes();

    // Check if all top-level nodes are either ClassDeclarationSyntax or NamespaceDeclarationSyntax
    foreach (var node in topLevelNodes)
    {
        if (node is not ClassDeclarationSyntax && node is not UsingDirectiveSyntax)
        {
            // Invalid top-level node found
            throw new InvalidOperationException($"Please pass in only plain-old-C#-object types in the input file (found a {node.Kind})");
        }
    }

    // All top-level nodes are valid
}

static ClassDeclarationSyntax[] GetClassDeclarations(string sourceCode)
{
    SyntaxNode root = GetSyntaxRoot(sourceCode);

    // Find all ClassDeclarationSyntax nodes in the tree
    IEnumerable<ClassDeclarationSyntax> classDeclarations = root
        .DescendantNodes()
        .OfType<ClassDeclarationSyntax>();

    return classDeclarations.ToArray();
}

// These four methods may not be needed, maybe just checking syntax errors is enough
static IEnumerable<TypeSyntax> GetPropertyTypes(ClassDeclarationSyntax classDeclaration)
{
    // Extract all property declarations from the class
    var propertyDeclarations = classDeclaration.Members
        .OfType<PropertyDeclarationSyntax>();

    // Return the TypeSyntax of each property
    return propertyDeclarations.Select(property => property.Type);
}

static IEnumerable<TypeSyntax> GetCollectionTypes(IEnumerable<TypeSyntax> typeSyntaxes)
{
    // Define a list of supported collection type patterns
    var supportedCollectionTypes = new List<Func<TypeSyntax, bool>>
    {
        // Match T[]
        type => type is ArrayTypeSyntax,

        // Match List<T>
        type => type is GenericNameSyntax genericName && genericName.Identifier.Text == "List",

        // Match Dictionary<TKey, TValue>
        type => type is GenericNameSyntax genericName && genericName.Identifier.Text == "Dictionary",

        // Match IEnumerable<T>
        type => type is GenericNameSyntax genericName && genericName.Identifier.Text == "IEnumerable",

        // Match IReadOnlyList<T>
        type => type is GenericNameSyntax genericName && genericName.Identifier.Text == "IReadOnlyList",

        // Match IReadOnlyDictionary<TKey, TValue>
        type => type is GenericNameSyntax genericName && genericName.Identifier.Text == "IReadOnlyDictionary"
    };

    // Filter the input types based on the supported collection type patterns
    return typeSyntaxes.Where(type => supportedCollectionTypes.Any(match => match(type)));
}

static IEnumerable<TypeSyntax> ExtractGenericParametersOrUnderlyingTypes(IEnumerable<TypeSyntax> typeSyntaxes)
{
    var extractedTypes = new List<TypeSyntax>();

    foreach (var type in typeSyntaxes)
    {
        switch (type)
        {
            case ArrayTypeSyntax arrayType:
                // For T[], extract the underlying type
                extractedTypes.Add(arrayType.ElementType);
                break;

            case GenericNameSyntax genericName:
                // For generic types like List<T>, Dictionary<TKey, TValue>, etc., extract the generic parameters
                extractedTypes.AddRange(genericName.TypeArgumentList.Arguments);
                break;

            default:
                // If the type doesn't match, ignore it (or handle other cases as needed)
                break;
        }
    }

    return extractedTypes;
}

static void ValidateClassPropertiesHaveValidTypeReferencesOrThrow(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var declaredTypes = classDeclarations.Select(c => c.Identifier.Text).ToHashSet();

    foreach (var classDeclaration in classDeclarations)
    {
        var propertyTypes = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(property => property.Type.ToString());
        var primitiveTypes = new HashSet<string> { "int", "string", "bool", "double", "float", "decimal", "DateTime" };
        var unknownTypeReferences = new List<string?>();

        foreach (var propertyType in propertyTypes)
        {
            // Check if the property type is not declared in the source file or imported namespaces
            if (!declaredTypes.Contains(propertyType))
            {
               unknownTypeReferences.Add(propertyType);
            }
        }

        if (unknownTypeReferences.Any())
        {
            throw new InvalidOperationException($"Class {classDeclaration.Identifier.Text} has properties with unknown types: {string.Join(", ", unknownTypeReferences)}");
        }
    }
}

static bool IsPoco(ClassDeclarationSyntax classDeclaration)
{
    // Check if the class has at least one public property  
    var publicProperties = classDeclaration.Members
        .OfType<PropertyDeclarationSyntax>()
        .Where(property =>
            property.Modifiers.Any(SyntaxKind.PublicKeyword) && // Public property  
            property.AccessorList != null && // Has accessors  
            property.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration) && // Has a getter  
            property.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration)); // Has a setter  

    if (!publicProperties.Any())
    {
        return false; // No public properties with public getters and setters  
    }

    // Check if all methods are allowed (Equals, GetHashCode, ToString)  
    var invalidMethods = classDeclaration.Members
        .OfType<MethodDeclarationSyntax>()
        .Where(method =>
            method.Modifiers.Any(SyntaxKind.PublicKeyword) && // Public method  
            method.Identifier.Text is not ("Equals" or "GetHashCode" or "ToString")); // Not an allowed method  

    if (invalidMethods.Any())
    {
        return false; // Found invalid methods  
    }

    // Check if properties with generic types have exactly one type parameter  
    foreach (var property in publicProperties)
    {
        if (property.Type is GenericNameSyntax genericType)
        {
            if (genericType.TypeArgumentList.Arguments.Count != 1)
            {
                return false; // Generic type does not have exactly one type parameter  
            }
        }
    }

    // Check if the class has 0 type parameters of its own
    if (classDeclaration.TypeParameterList != null && classDeclaration.TypeParameterList.Parameters.Count > 0)
    {
        return false; // Class has type parameters
    }

    // If all checks pass, the class is a POCO  
    return true;
}

static void AllClassDeclarationsArePocosOrThrow(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    // Check if all class declarations are POCOs
    foreach (var classDeclaration in classDeclarations)
    {
        if (!IsPoco(classDeclaration))
        {
            throw new InvalidOperationException($"Class {classDeclaration.Identifier.Text} is not a POCO.");
        }
    }
}

static void ValidateSqlTableNameAttributesOrThrow(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    foreach (var classDeclaration in classDeclarations)
    {
        // Check if the class has the SqlTableNameAttribute
        var attributes = classDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Where(attr => attr.Name.ToString() == "SqlTableName");

        if (!attributes.Any())
        {
            throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' is missing the required SqlTableNameAttribute.");
        }

        foreach (var attribute in attributes)
        {
            // Get the arguments of the SqlTableNameAttribute
            var arguments = attribute.ArgumentList?.Arguments;

            if (arguments == null || arguments.Value.Count != 3)
            {
                throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' has an invalid SqlTableNameAttribute. It must have exactly three string arguments.");
            }

            // Validate the arguments are non-null, non-empty, and non-whitespace
            foreach (var argument in arguments)
            {
                if (argument.Expression is LiteralExpressionSyntax literalExpression &&
                    literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    var stringValue = literalExpression.Token.ValueText;
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' has an invalid SqlTableNameAttribute. Both parameters must be non-null, non-empty, and non-whitespace strings.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' has an invalid SqlTableNameAttribute. Both parameters must be string literals.");
                }
            }
        }
    }
}

static ClassDeclarationSyntax ValidateSinglePrimaryKey(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var classesWithPrimaryKey = classDeclarations
        .Where(classDeclaration => classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Any(property => property.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString() == "PrimaryKey")))
        .ToList();

    if (classesWithPrimaryKey.Count != 1)
    {
        throw new InvalidOperationException($"There must be exactly one class with a property marked with the PrimaryKeyAttribute. Found {classesWithPrimaryKey.Count}.");
    }

    var primaryKeyClass = classesWithPrimaryKey.Single();
    var primaryKeyProperties = primaryKeyClass.Members
        .OfType<PropertyDeclarationSyntax>()
        .Where(property => property.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString() == "PrimaryKey"))
        .ToList();

    if (primaryKeyProperties.Count != 1)
    {
        throw new InvalidOperationException($"Class '{primaryKeyClass.Identifier.Text}' must have exactly one property with the PrimaryKeyAttribute. Found {primaryKeyProperties.Count}.");
    }

    return primaryKeyClass;
}

static void ValidateForeignKeyForAllOtherClasses(IEnumerable<ClassDeclarationSyntax> classDeclarations, ClassDeclarationSyntax primaryKeyClass)
{
    var otherClasses = classDeclarations.Where(c => c != primaryKeyClass);

    foreach (var classDeclaration in otherClasses)
    {
        var foreignKeyProperties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(property => property.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString() == "ForeignKey"))
            .ToList();

        if (foreignKeyProperties.Count != 1)
        {
            throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' must have exactly one property with the ForeignKeyAttribute. Found {foreignKeyProperties.Count}.");
        }
    }
}

static void ValidateForeignKeyAttributeArguments(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    foreach (var classDeclaration in classDeclarations)
    {
        var foreignKeyProperties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(property => property.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString() == "ForeignKey"));

        foreach (var property in foreignKeyProperties)
        {
            var foreignKeyAttribute = property.AttributeLists
                .SelectMany(al => al.Attributes)
                .First(attr => attr.Name.ToString() == "ForeignKey");

            var arguments = foreignKeyAttribute.ArgumentList?.Arguments;

            if (arguments == null || arguments.Value.Count != 1)
            {
                throw new InvalidOperationException($"Property '{property.Identifier.Text}' in class '{classDeclaration.Identifier.Text}' has an invalid ForeignKeyAttribute. It must have exactly one string argument.");
            }

            if (arguments.Value[0].Expression is LiteralExpressionSyntax literalExpression &&
                literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var stringValue = literalExpression.Token.ValueText;
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    throw new InvalidOperationException($"Property '{property.Identifier.Text}' in class '{classDeclaration.Identifier.Text}' has a ForeignKeyAttribute with an invalid argument. The argument must be a non-null, non-empty string.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Property '{property.Identifier.Text}' in class '{classDeclaration.Identifier.Text}' has a ForeignKeyAttribute with an invalid argument. The argument must be a string literal.");
            }
        }
    }
}

#endregion

#region Build and Validate Type DAG

// Type DAG basics:
// - Each class is a node
// - A directed edge says that class A has a property or collection of class B
// - We only care about edges between types in our source file (i.e. if A has a member of int, we don't
//   add it to the graph)
// - We only care about edges between classes that are POCOs in our file
// - Types that appear as generic types in collections are also added to the graph
// - There must be no cycles in the graph
// - Each node must have exactly one parent (i.e. type A cannot have two properties of type B if type
//   C also has a property of type B)
// - Therefore, the graph is a tree (no cycles, no multiple parents)
// - There must be one tree (that is, all classes must be connected)

// Diagnostics:
// - In order to tell the user exactly how their file is invalid, we need to be able to
//   construct the full graph as it is, cycles and multiple parents and all, so we can
//   show them exactly what is wrong. So we end up needing two sets of data structures.
// - For full diagnostics, we want to be able to print the proper member names (i.e. T A.B),
//   so the graph ends of being two kinds of nodes: types and properties, which kind of sucks.
// - But let's go with it. The types for the diagnostic graph:
//   - TypeDiagnosticNode: (string typeName, List<PropertyDiagnosticNode> properties, List<PropertyDiagnosticNode> parents)
//   - PropertyDiagnosticNode: (string propertyName, TypeDiagnosticNode type, List<TypeDiagnosticNode> parents)
// - Note that, by "type" we mean "underlying type" (i.e. T => T, T[] => T, List<T> => T, etc.)
// - So we end up with an alternating pattern of TypeDiagnosticNode and PropertyDiagnosticNode
static IEnumerable<TypeDiagnosticNode> BuildEmptyTypeDiagnosticNodes(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    foreach (var classDeclaration in classDeclarations)
    {
        var typeName = classDeclaration.Identifier.Text;
        yield return new TypeDiagnosticNode(typeName);
    }
}

static string GetUnderlyingTypeName(TypeSyntax type)
{
    return type switch
    {
        ArrayTypeSyntax arrayType => arrayType.ElementType.ToString(),
        GenericNameSyntax genericName => genericName.TypeArgumentList.Arguments.Single().ToString(),
        _ => type.ToString()
    };
}

static bool IsTypeInFile(string typeName, IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    return classDeclarations.Any(c => c.Identifier.Text == typeName);
}

static IEnumerable<PropertyDiagnosticNode> BuildPropertiesForClass(
    ClassDeclarationSyntax classDeclaration,
    IEnumerable<TypeDiagnosticNode> typeNodes,
    IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var propertyDeclarations = classDeclaration.Members
        .OfType<PropertyDeclarationSyntax>();
    foreach (var property in propertyDeclarations)
    {
        var propertyName = property.Identifier.Text;
        var propertyType = GetUnderlyingTypeName(property.Type);
        // Check if the property type is a class in the source file
        if (IsTypeInFile(propertyType, classDeclarations))
        {
            var typeNode = typeNodes.First(t => t.TypeName == propertyType);
            var propertyNode = new PropertyDiagnosticNode(propertyName, typeNode);
            yield return propertyNode;
        }
    }
}

static IEnumerable<TypeDiagnosticNode> BuildDiagnosticGraph(IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var types = BuildEmptyTypeDiagnosticNodes(classDeclarations).ToArray();
    foreach (var classDeclaration in classDeclarations)
    {
        var typeName = classDeclaration.Identifier.Text;
        var typeNode = types.First(t => t.TypeName == typeName);
        var propertyNodes = BuildPropertiesForClass(classDeclaration, types, classDeclarations);
        foreach (var propertyNode in propertyNodes)
        {
            typeNode.AddProperty(propertyNode);
        }
    }
    return types;
}

static void DetectCycleInGraph(TypeDiagnosticNode startNode,
    HashSet<TypeDiagnosticNode> visitedNodes,
    Stack<string> propertyStack)
{
    // If the node has already been visited in this traversal, a cycle is detected
    if (visitedNodes.Contains(startNode))
    {
        throw new InvalidOperationException($"Invalid type hierarchy: The path of properties '{string.Join(" -> ", propertyStack)}' contains a circular reference. Circular references are not allowed.");
    }

    // Mark the current node as visited
    visitedNodes.Add(startNode);

    // Get the current type's name so we can format steps like T A.B.
    var typeName = startNode.TypeName;

    // Traverse the properties of the current node
    foreach (var property in startNode.Properties)
    {
        // Push the property name onto the stack
        propertyStack.Push($"{property.PropertyType.TypeName} {typeName}.{property.PropertyName}");

        // Recursively check the type of the property
        DetectCycleInGraph(property.PropertyType, visitedNodes, propertyStack);

        // Pop the property name off the stack after returning from recursion
        propertyStack.Pop();
    }

    // Unmark the current node as visited for other traversal paths
    visitedNodes.Remove(startNode);
}

static void DetectCyclesInGraph(IEnumerable<TypeDiagnosticNode> typeNodes)
{
    var visitedNodes = new HashSet<TypeDiagnosticNode>();
    var propertyStack = new Stack<string>();
    var cycleDetectionExceptions = new List<InvalidOperationException>();
    foreach (var typeNode in typeNodes)
    {
        try
        {
            // Start the cycle detection from each node
            DetectCycleInGraph(typeNode, visitedNodes, propertyStack);
        }
        catch (InvalidOperationException ex)
        {
            // Catch the cycle detection exception and add it to the list
            cycleDetectionExceptions.Add(ex);
        }
    }

    if (cycleDetectionExceptions.Any())
    {
        // If any cycle detection exceptions were caught, throw a new exception with all the messages
        throw new InvalidOperationException($"Type hierarchy contains {cycleDetectionExceptions.Count} circular references:\n{string.Join("\n", cycleDetectionExceptions.Select(e => e.Message))}");
    }
}

static void ValidateSingleParentPerTypeOrThrow(IEnumerable<TypeDiagnosticNode> typeNodes)
{
    var errorMessages = new List<string>();

    foreach (var typeNode in typeNodes)
    {
        // Check the number of parent properties for the current type
        if (typeNode.Parents.Count != 1)
        {
            // Build a descriptive error message
            var parentDescriptions = typeNode.Parents
                .Select(parent => $"{parent.PropertyType.TypeName}.{parent.PropertyName}")
                .ToList();

            var message = parentDescriptions.Count == 0
                ? $"The type '{typeNode.TypeName}' is not used as a property in any other type. Each type must appear exactly once as a property."
                : $"The type '{typeNode.TypeName}' is a property on the following types: {string.Join(", ", parentDescriptions)}. Each type must appear exactly once as a property.";

            errorMessages.Add(message);
        }
    }

    if (errorMessages.Any())
    {
        // If any error messages were collected, throw a new exception with all the messages
        throw new InvalidOperationException($"Type hierarchy contains {errorMessages.Count} types with multiple parents:\n{string.Join("\n", errorMessages)}");
    }
}

static void ValidateSingleTreeOrThrow(IEnumerable<TypeDiagnosticNode> typeNodes)
{
    // Check if there is more than one root node in the graph
    var rootNodes = typeNodes.Where(t => t.Parents.Count == 0).ToList();
    if (rootNodes.Count > 1)
    {
        throw new InvalidOperationException($"The type hierarchy contains multiple disconnected sets of types: {string.Join(", ", rootNodes.Select(t => t.TypeName))}. All types in file must reference each other.");
    }
}

static void ValidateDiagnosticGraph(IEnumerable<TypeDiagnosticNode> typeNodes)
{
    // Check for cycles in the graph
    DetectCyclesInGraph(typeNodes);
    // Check for multiple parents per type
    ValidateSingleParentPerTypeOrThrow(typeNodes);
    // Check for a single tree in the graph
    ValidateSingleTreeOrThrow(typeNodes);
}
#endregion

#region Build Projection Type
// Build the projection type, which represents one row of a SQL query that LEFT JOINS every single
// type in the type graph. Each property of a given type T in the original type hierarchy is replaced
// with its properties directly in the projection type until all properties have no type in the type
// hierarchy. These blocks of properties track their "source" types, and all except the properties
// of the root type are nullable.

// To build the projection type, we need to:
// 1. Get the root type of the type hierarchy
// 2. Put all its properties into a list
// 3. Repeatedly iterate over the list, replacing a property if it's underlying type is in the type
//    hierarchy. We replace it with the properties of that type, and add the new properties to the list.
// 4. Iterate until there are no more properties to replace.
static IReadOnlyDictionary<TypeDiagnosticNode, ClassDeclarationSyntax> AssociateTypeGraphWithDeclarations(
    IEnumerable<TypeDiagnosticNode> typeNodes, IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var typeToClassMap = new Dictionary<TypeDiagnosticNode, ClassDeclarationSyntax>();
    foreach (var classDeclaration in classDeclarations)
    {
        var typeName = classDeclaration.Identifier.Text;
        var typeNode = typeNodes.FirstOrDefault(t => t.TypeName == typeName);
        if (typeNode != null)
        {
            typeToClassMap[typeNode] = classDeclaration;
        }
    }
    return typeToClassMap;
}

static TypeDiagnosticNode GetRootNode(IEnumerable<TypeDiagnosticNode> typeDiagnosticNodes)
{
    // Find the root node (the one with no parents)
    var rootNode = typeDiagnosticNodes.FirstOrDefault(t => t.Parents.Count == 0);
    if (rootNode == null)
    {
        throw new InvalidOperationException("No root node found in the type hierarchy.");
    }
    return rootNode;
}

static IReadOnlyList<PropertyDeclarationFromType> GetPropertiesFromClassDeclaration(ClassDeclarationSyntax classDeclaration)
{
    return classDeclaration.Members
        .OfType<PropertyDeclarationSyntax>()
        .Select(property => new PropertyDeclarationFromType(property, classDeclaration.Identifier.Text))
        .ToList();
}

static bool PropertyReplacementsRequired(IEnumerable<PropertyDeclarationFromType> properties, IEnumerable<TypeDiagnosticNode> typeNodes)
{
    // Check if any property in the list has an underlying type that is in the type hierarchy
    return properties.Any(property => typeNodes.Any(typeNode => typeNode.TypeName == GetUnderlyingTypeName(property.PropertyDeclaration.Type)));
}

// Fix for CS8422: Remove 'this' reference in static local function
static PropertyDeclarationSyntax RewritePropertyTypeAsNullableIfCLRStruct(PropertyDeclarationSyntax propertyDeclaration)
{
    // Check if the type is one of the CLR primitive struct types
    var primitiveTypes = new HashSet<string>
    {
        "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
        "float", "double", "decimal", "char", "DateTime", "DateTimeOffset",
        "TimeSpan", "Guid", "bool"
    };
    var typeName = propertyDeclaration.Type.ToString();
    if (primitiveTypes.Contains(typeName))
    {
        // If the type is a primitive struct, make it nullable
        var nullableType = SyntaxFactory.NullableType(propertyDeclaration.Type);
        return propertyDeclaration.WithType(nullableType);
    }
    else
    {
        // If the type is not a primitive struct, return the original property declaration
        return propertyDeclaration;
    }
}

static IReadOnlyList<PropertyDeclarationFromType> ReplacePropertiesInTypeHierarchySingleStep(
    IEnumerable<PropertyDeclarationFromType> properties,
    IReadOnlyDictionary<TypeDiagnosticNode, ClassDeclarationSyntax> typeToClassMap)
{
    var newProperties = new List<PropertyDeclarationFromType>();
    foreach (var property in properties)
    {
        var underlyingTypeName = GetUnderlyingTypeName(property.PropertyDeclaration.Type);
        var typeNode = typeToClassMap.Keys.FirstOrDefault(t => t.TypeName == underlyingTypeName);
        if (typeNode != null)
        {
            // If the property type is in the type hierarchy, replace it with its properties
            var classDeclaration = typeToClassMap[typeNode];
            var newPropertyDeclarations = GetPropertiesFromClassDeclaration(classDeclaration)
                .Select(p => new PropertyDeclarationFromType(
                    RewritePropertyTypeAsNullableIfCLRStruct(p.PropertyDeclaration),
                    p.FromTypeName));
            newProperties.AddRange(newPropertyDeclarations);
        }
        else
        {
            // If the property type is not in the type hierarchy, keep it as is
            newProperties.Add(property);
        }
    }
    return newProperties;
}

static IEnumerable<PropertyDeclarationFromType> BuildPropertiesForProjectionType(IEnumerable<ClassDeclarationSyntax> classDeclarations,
    IEnumerable<TypeDiagnosticNode> typeNodes)
{
    var typeToClassMap = AssociateTypeGraphWithDeclarations(typeNodes, classDeclarations);
    var rootNode = GetRootNode(typeNodes);
    var propertyList = GetPropertiesFromClassDeclaration(typeToClassMap[rootNode]);

    // Iterate until there are no more properties to replace
    while (PropertyReplacementsRequired(propertyList, typeNodes))
    {
        propertyList = ReplacePropertiesInTypeHierarchySingleStep(propertyList, typeToClassMap);
    }

    return propertyList;
}

static ClassDeclarationSyntax BuildProjectionType(IEnumerable<PropertyDeclarationFromType> properties,
    IEnumerable<TypeDiagnosticNode> typeNodes)
{
    var rootNode = GetRootNode(typeNodes);
    var projectionTypeName = rootNode.TypeName + "Item";
    // Create a new class declaration for the projection type
    var projectionClass = SyntaxFactory.ClassDeclaration(projectionTypeName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword));
    // Add properties to the projection class
    foreach (var property in properties)
    {
        var propertyDeclaration = property.PropertyDeclaration
           .WithIdentifier(SyntaxFactory.Identifier(property.PropertyDeclaration.Identifier.Text))
           .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
           .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
           [
               SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                   .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
               SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                   .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                   .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
           ])));
        projectionClass = projectionClass.AddMembers(propertyDeclaration);
    }
    return projectionClass;
}
#endregion

#region Build SELECT Query
static SqlTableInfo GetTableInfoFromClassDeclaration(ClassDeclarationSyntax classDeclaration)
{
    var attributes = classDeclaration.AttributeLists
        .SelectMany(al => al.Attributes)
        .Where(attr => attr.Name.ToString() == "SqlTableName");
    if (!attributes.Any())
    {
        throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' is missing the required SqlTableNameAttribute.");
    }
    var attribute = attributes.First();
    var arguments = attribute.ArgumentList?.Arguments;
    if (arguments == null || arguments.Value.Count != 3)
    {
        throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' has an invalid SqlTableNameAttribute. It must have exactly three string arguments.");
    }
    var schemaName = arguments.Value[0].Expression.ToString().Trim('"');
    var tableName = arguments.Value[1].Expression.ToString().Trim('"');
    var joinAlias = arguments.Value[2].Expression.ToString().Trim('"');
    return new SqlTableInfo(schemaName, tableName, joinAlias);
}

static string GetFROMClauseForRootType(TypeDiagnosticNode rootNode, IEnumerable<ClassDeclarationSyntax> classDeclarations)
{
    var tableInfo = GetTableInfoFromClassDeclaration(classDeclarations.First(c => c.Identifier.Text == rootNode.TypeName));
    var fromClause = $"FROM {tableInfo.SchemaName}.{tableInfo.TableName} AS {tableInfo.JoinAlias}";
    return fromClause;
}

static IEnumerable<string> GetTypeNamesInJoinOrder(IEnumerable<PropertyDeclarationFromType> propertyDeclarations)
{
    var typeNames = new List<string>();
    foreach (var property in propertyDeclarations)
    {
        var typeName = property.FromTypeName;
        var lastTypeName = LastTypeName(typeNames);
        if (typeName != lastTypeName)
        {
            typeNames.Add(typeName);
        }
    }

    return typeNames;

    static string? LastTypeName(IReadOnlyList<string> strings)
    {
        return strings.Count > 0 ? strings[^1] : null;
    }
}

static PropertyDeclarationSyntax GetForeignKeyProperty(ClassDeclarationSyntax classDeclaration)
{
    // Find all properties in the class
    var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();

    // Filter properties to find the one with the ForeignKeyAttribute
    var foreignKeyProperties = properties.Where(property =>
        property.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString() == "ForeignKey"));

    // Ensure there is exactly one property with the ForeignKeyAttribute
    if (foreignKeyProperties.Count() != 1)
    {
        throw new InvalidOperationException($"Class '{classDeclaration.Identifier.Text}' must have exactly one property with the ForeignKeyAttribute.");
    }

    return foreignKeyProperties.Single();
}

static AttributeSyntax GetForeignKeyAttributeFromProperty(PropertyDeclarationSyntax propertyDeclaration)
{
    // Find the ForeignKeyAttribute from the property declaration
    var foreignKeyAttributes = propertyDeclaration.AttributeLists
        .SelectMany(al => al.Attributes)
        .Where(attr => attr.Name.ToString() == "ForeignKey");
    // Ensure there is exactly one ForeignKeyAttribute
    if (foreignKeyAttributes.Count() != 1)
    {
        throw new InvalidOperationException($"Property '{propertyDeclaration.Identifier.Text}' must have exactly one ForeignKeyAttribute.");
    }
    return foreignKeyAttributes.Single();
}

static string BuildLeftJoinClauseForClass(ClassDeclarationSyntax classDeclaration,
    IReadOnlyDictionary<TypeDiagnosticNode, ClassDeclarationSyntax> typeToClassMap)
{
    // Get the table info of the class declarations
    var tableInfo = GetTableInfoFromClassDeclaration(classDeclaration);
    // Find the diagnostic node of the class declaration
    var typeNode = typeToClassMap.Keys.Single(t => t.TypeName == classDeclaration.Identifier.Text);
    // Find the foreign key property in the class declaration...
    var foreignKeyProperty = GetForeignKeyProperty(classDeclaration);
    // ...and the foreign key attribute from the property
    var foreignKeyAttribute = GetForeignKeyAttributeFromProperty(foreignKeyProperty);
    // Find the parent type of the class declaration in the hierarchy
    var parentTypeNode = typeNode.Parents.Single();

    // Get the values of the ForeignKeyAttribute (one string literal)
    var foreignKeyAttributeArguments = foreignKeyAttribute.ArgumentList?.Arguments;
    var foreignKeyValue = foreignKeyAttributeArguments?[0].Expression.ToString().Trim('"');

    // Find the ClassDeclaration of the parent type node
    var parentClassDeclaration = typeToClassMap[parentTypeNode.Parents.Single()];

    // Find the property on the parent class declaration that matches foreignKeyValue
    var parentProperty = parentClassDeclaration.Members
        .OfType<PropertyDeclarationSyntax>()
        .FirstOrDefault(p => p.Identifier.Text == foreignKeyValue);

    // Throw if the property is not found
    if (parentProperty == null)
    {
        throw new InvalidOperationException($"Property '{foreignKeyValue}' was declared as the foreign key for {typeNode.TypeName}, but was not present on {parentClassDeclaration.Identifier.Text}.");
    }

    // Get the SqlTableInfo for the parent class and the class declaration.
    var parentTableInfo = GetTableInfoFromClassDeclaration(parentClassDeclaration);
    var classTableInfo = GetTableInfoFromClassDeclaration(classDeclaration);

    var join = $"LEFT JOIN {classTableInfo.SchemaName}.{classTableInfo.TableName} {classTableInfo.JoinAlias}"
        + $" ON {classTableInfo.JoinAlias}.{foreignKeyProperty.Identifier.Text} = {parentTableInfo.JoinAlias}.{parentProperty.Identifier.Text}";
    return join;
}
#endregion

internal record PropertyDeclarationFromType(PropertyDeclarationSyntax PropertyDeclaration, string FromTypeName);
internal record SqlTableInfo(string SchemaName, string TableName, string JoinAlias);