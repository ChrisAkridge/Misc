using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToCixTransform
{
	internal sealed class TreeBuilder
    {
        private readonly Random random = new Random();
        private readonly List<ClassTransform> classTransforms = new List<ClassTransform>();
        private readonly List<VariableDeclaration> globals = new List<VariableDeclaration>();
        private readonly List<Statement> globalInitializerStatements = new List<Statement>();
        private readonly List<ClassVTableStructDefinition> interfaces = new List<ClassVTableStructDefinition>();
        private readonly Dictionary<string, int> enumDefines = new Dictionary<string, int>();
        private SemanticModel model;

        public IReadOnlyList<ClassTransform> ClassTransforms => classTransforms.AsReadOnly();
        public IReadOnlyList<VariableDeclaration> Globals => globals.AsReadOnly();
        public IReadOnlyList<Statement> GlobalInitializerStatements => globalInitializerStatements.AsReadOnly();
        public IReadOnlyList<ClassVTableStructDefinition> Interfaces => interfaces.AsReadOnly();
        public IReadOnlyDictionary<string, int> EnumDefines => new ReadOnlyDictionary<string, int>(enumDefines);

        public void BuildTree(string programText)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(programText);
            var root = syntaxTree.GetCompilationUnitRoot();
            var compilation = CSharpCompilation.Create("Program")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) { throw new Exception("Errors were present in the source code."); }

            model = compilation.GetSemanticModel(syntaxTree);

            foreach (var node in root.Members)
            {
                switch (node)
                {
                    case ClassDeclarationSyntax classDeclaration:
                        VisitClass(classDeclaration);
                        break;

                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        VisitInterface(interfaceDeclaration);
                        break;

                    case EnumDeclarationSyntax enumDeclaration:
                        VisitEnum(enumDeclaration);
                        break;
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void VisitClass(TypeDeclarationSyntax classDeclaration, string parentClassName = "")
        {
            if (classDeclaration.Arity > 0) { return; }

            var classSymbol = model.GetDeclaredSymbol(classDeclaration);

            var className = (parentClassName == string.Empty)
                ? classDeclaration.Identifier.Text
                : parentClassName + "_" + classDeclaration.Identifier.Text;
            var childClasses = classDeclaration.Members.Where(m => m is ClassDeclarationSyntax).OfType<ClassDeclarationSyntax>();
            var fields = classDeclaration.Members.Where(m => m is FieldDeclarationSyntax).OfType<FieldDeclarationSyntax>();
            var properties = classDeclaration.Members.Where(m => m is PropertyDeclarationSyntax).OfType<PropertyDeclarationSyntax>();
            var constructors = classDeclaration.Members.Where(m => m is ConstructorDeclarationSyntax)
                .OfType<ConstructorDeclarationSyntax>();
            var methods = classDeclaration.Members.Where(m => m is MethodDeclarationSyntax).OfType<MethodDeclarationSyntax>();

            var staticFields = fields.Where(f => f.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword));
            var staticProperties = properties.Where(p => p.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword));
            var staticConstructors =
                constructors.Where(c => c.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword));
            var staticMethods = methods.Where(method => method.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword));

            fields = fields.Except(staticFields);
            properties = properties.Except(staticProperties);
            constructors = constructors.Except(staticConstructors);
            methods = methods.Except(staticMethods);

            var transform = new ClassTransform
            {
                ClassName = className,
                Members = new ClassMemberStruct { Members = fields.Select(VisitField).ToList(), Name = className },
                TypeId = random.Next(int.MinValue, int.MaxValue),
                Functions = methods
                    .Select(m => VisitMethod(m, className))
                    .Concat(properties.Select(VisitProperty).SelectMany(f => f).Where(f => f != null))
                    .ToList(),
                StaticFunctions = staticProperties.Select(VisitStaticProperty).SelectMany(f => f).Where(f => f != null)
                    .Concat(staticMethods.Select(VisitStaticMethod))
                    .Concat(constructors.Select(VisitConstructor))
                    .ToList(),
                CatchBlockCaptures = new List<CatchBlockCapturesStruct>(),
                CatchBlockFunctions = new List<StaticMethodFunction>(),
                Inheritance = new InheritanceRegistration
                {
                    BaseTypeName = classSymbol.BaseType.Name,
                    DerivedTypeName = className
                }
            };

            transform.VTable = new ClassVTableStructDefinition
            {
                Name = className + "_vtable_struct",
                Pointers = transform.Functions.Select(f => new ClassVTableFunctionPointer
                {
                    FunctionName = f.Name,
                    ReturnTypeName = f.ReturnTypeName,
                    ParameterTypeNames = f.Parameters.Select(p => p.TypeName).ToList()
                }).ToList()
            };
            globalInitializerStatements.AddRange(staticConstructors.Select(c => VisitBlock(c.Body)));

            foreach (var childClass in childClasses) { VisitClass(childClass, className); }
            classTransforms.Add(transform);

            globals.AddRange(staticFields.Select(VisitStaticField).Cast<VariableDeclaration>());
        }

        public StaticMethodFunction VisitStaticMethod(MethodDeclarationSyntax methodDeclaration)
        {
            var containingType = model.GetDeclaredSymbol(methodDeclaration).ContainingType;

            return new StaticMethodFunction
            {
                MethodOf = VisitType(containingType),
                Name = methodDeclaration.Identifier.Text,
                Parameters = methodDeclaration.ParameterList.Parameters.Select(VisitMethodParameter).ToList(),
                ReturnTypeName = VisitType(methodDeclaration.ReturnType, true, false),
                Statements = new List<Statement> { VisitBlock(methodDeclaration.Body) }
            };
        }

        public StaticMethodFunction[] VisitStaticProperty(PropertyDeclarationSyntax prop)
        {
            var containingType = model.GetSymbolInfo(prop).Symbol.ContainingType;
            var returnType = prop.Type;
            var propertyName = prop.Identifier.Text;

            var getter = prop.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            var setter = prop.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);

            StaticMethodFunction getterFunction = null;
            StaticMethodFunction setterFunction = null;

            var returnTypeName = VisitType(returnType, true, false);
            if (getter != null)
            {
                getterFunction = new StaticMethodFunction
                {
                    MethodOf = VisitType(containingType),
                    Name = $"{prop.Identifier.Text}_get",
                    Parameters = new List<MethodParameter>(),
                    ReturnTypeName = returnTypeName
                };

                if (getter.Body != null)
                {
                    getterFunction.Statements = new List<Statement>
                    {
                        VisitBlock(getter.Body)
                    };
                }
                else
                {
                    getterFunction.Statements = new List<Statement>
                    {
                        new ReturnValueStatement
                        {
                            Expression = new Literal
                            {
                                LiteralText = $"{getterFunction.MethodOf}_{propertyName}",
                                TypeName = returnTypeName
                            }
                        }
                    };
                }
            }

            if (setter != null)
            {
                setterFunction = new StaticMethodFunction
                {
                    MethodOf = VisitType(containingType),
                    Name = $"{prop.Identifier.Text}_get",
                    Parameters = new List<MethodParameter>
                    {
                        new MethodParameter
                        {
                            Name = "value",
                            TypeName = returnTypeName
                        }
                    },
                    ReturnTypeName = returnTypeName
                };

                if (setter.Body != null)
                {
                    setterFunction.Statements = new List<Statement>
                    {
                        VisitBlock(setter.Body)
                    };
                }
                else
                {
                    setterFunction.Statements = new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = new BinaryExpression
                            {
                                Left = new Literal
                                {
                                    LiteralText = $"{setterFunction.MethodOf}_{propertyName}",
                                    TypeName = returnTypeName
                                },
                                Right = new Literal
                                {
                                    LiteralText = "value",
                                    TypeName = returnTypeName
                                },
                                OperatorSymbol = "="
                            }
                        }
                    };
                }
            }

            return new[] { getterFunction, setterFunction };
        }

        public StructMember VisitField(FieldDeclarationSyntax fieldDeclaration)
        {
            var typeName = VisitType(fieldDeclaration.Declaration.Type, true, false);
            var fieldName = fieldDeclaration.Declaration.Variables[0].Identifier.Text;

            return new StructMember
            {
                Name = fieldName,
                TypeName = typeName
            };
        }

        public StaticMethodFunction VisitConstructor(ConstructorDeclarationSyntax constructorDeclaration)
        {
            var containingTypeName = VisitType(model.GetDeclaredSymbol(constructorDeclaration).ContainingType);
            var thisParameter = new MethodParameter { Name = "this", TypeName = containingTypeName };
            return new StaticMethodFunction
            {
                MethodOf = containingTypeName,
                Name = "ctor",
                Parameters = constructorDeclaration.ParameterList.Parameters.Select(VisitMethodParameter)
                    .Prepend(thisParameter).ToList(),
                Statements = new List<Statement> { VisitBlock(constructorDeclaration.Body) }
            };
        }

        public string VisitType(TypeSyntax type, bool appendPointers, bool capitalizePredefinedTypes)
        {
            switch (type)
            {
                case GenericNameSyntax genericName:
                {
                    var typeNames = genericName.TypeArgumentList.Arguments.Select(t => VisitType(t, false, true));
                    var newTypeName = genericName.Identifier.Text + "Of" + string.Join("", typeNames);
                    if (appendPointers) { newTypeName += "*"; }
                    return newTypeName;
                }

                case IdentifierNameSyntax identifier:
                {
                    var identifierText = identifier.Identifier.Text;
                    if (appendPointers && identifierText != "DateTimeOffset") { identifierText += "*"; }
                    return identifierText;
                }

                case PredefinedTypeSyntax predefinedType:
                {
                    var typeName = predefinedType.Keyword.Text;
                    switch (typeName)
                    {
                        case "bool": return "int";
                        case "T": return "void*";
                        case "string": return "String*";
                        default:
                            return (capitalizePredefinedTypes)
                                ? typeName.First().ToString().ToUpper() + typeName.Substring(1)
                                : typeName;
                    }
                }

                case ArrayTypeSyntax arrayType:
                {
                    var visitType = "ArrayOf" + VisitType(arrayType.ElementType, false, true);
                    if (appendPointers) { visitType += "*"; }
                    return visitType;
                }

                case RefTypeSyntax refType: return VisitType(refType.Type, false, false) + "*";
                case QualifiedNameSyntax qualifiedName: return VisitQualifiedName(qualifiedName);
                default: throw new ArgumentException();
            }
        }

        public string VisitType(ITypeSymbol type)
        {
            switch (type)
            {
                case INamedTypeSymbol namedType: return VisitType(namedType);
                case IArrayTypeSymbol arrayType: return $"ArrayOf{VisitType(arrayType.ElementType)}";
                default: return null;
            }
        }

        public string VisitType(INamedTypeSymbol type)
        {
            var name = type.Name;

            if (type.TypeArguments.Any())
            {
                name += $"Of{string.Join("", type.TypeArguments.Select(VisitType))}";
            }

            return name;
        }

        public string VisitQualifiedName(QualifiedNameSyntax qualifiedName)
        {
            var left = VisitType(qualifiedName.Left, false, true);
            var right = VisitType(qualifiedName.Right, false, true);

            return left + "_" + right;
        }

        public InstanceMethodFunction[] VisitProperty(PropertyDeclarationSyntax propertyDeclaration)
        {
            var containingType = model.GetDeclaredSymbol(propertyDeclaration).ContainingType;
            var returnType = propertyDeclaration.Type;

            var getter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            var setter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);

            InstanceMethodFunction getterFunction = null;
            InstanceMethodFunction setterFunction = null;

            var propertyTypeName = VisitType(returnType, true, false);
            if (getter != null)
            {
                getterFunction = new InstanceMethodFunction
                {
                    MethodOf = VisitType(containingType),
                    Name = $"{propertyDeclaration.Identifier.Text}_get",
                    Parameters = new List<MethodParameter>
                    {
                        new MethodParameter { Name = "this", TypeName = VisitType(containingType) + "*" }
                    },
                    ReturnTypeName = propertyTypeName
                };

                if (getter.Body != null)
                {
                    getterFunction.Statements = new List<Statement>
                    {
                        VisitBlock(getter.Body)
                    };
                }
                else
                {
                    getterFunction.Statements = new List<Statement>
                    {
                        new ReturnValueStatement
                        {
                            Expression = new Literal
                            {
                                LiteralText = $"{getterFunction.MethodOf}_{getterFunction.Name}",
                                TypeName = propertyTypeName
                            }
                        }
                    };
                }
            }

            if (setter == null) { return new[] { getterFunction, null }; }

            setterFunction = new InstanceMethodFunction
            {
                MethodOf = VisitType(containingType),
                Name = $"{propertyDeclaration.Identifier.Text}_set",
                Parameters = new List<MethodParameter>
                {
                    new MethodParameter { Name = "this", TypeName = VisitType(containingType) + "*" },
                    new MethodParameter
                    {
                        Name = "value",
                        TypeName = propertyTypeName
                    }
                },
                ReturnTypeName = propertyTypeName
            };

            if (setter.Body != null)
            {
                setterFunction.Statements = new List<Statement>
                {
                    VisitBlock(setter.Body)
                };
            }
            else
            {
                setterFunction.Statements = new List<Statement>
                {
                    new ExpressionStatement
                    {
                        Expression = new BinaryExpression
                        {
                            Left = new Literal
                            {
                                LiteralText = $"{setterFunction.MethodOf}_{setterFunction.Name}",
                                TypeName = propertyTypeName
                            },
                            Right = new Literal
                            {
                                LiteralText = "value",
                                TypeName = propertyTypeName
                            },
                            OperatorSymbol = "="
                        }
                    }
                };
            }

            return new[] { getterFunction, setterFunction };
        }

        public ClassVTableFunctionPointer VisitMethodForVTable(MethodDeclarationSyntax methodDeclaration, string className)
        {
            var returnType = VisitType(methodDeclaration.ReturnType, true, false);
            var parameters = methodDeclaration.ParameterList.Parameters.Select(param => VisitType(param.Type, true, false));
            parameters = parameters.Prepend(className + "*");
            var functionName = methodDeclaration.Identifier.Text;

            return new ClassVTableFunctionPointer
            {
                FunctionName = functionName,
                ParameterTypeNames = parameters.ToList(),
                ReturnTypeName = returnType
            };
        }

        public InstanceMethodFunction VisitMethod(MethodDeclarationSyntax methodDeclaration, string className)
        {
            var function = new InstanceMethodFunction
            {
                MethodOf = className,
                Name = methodDeclaration.Identifier.Text,
                ReturnTypeName = VisitType(methodDeclaration.ReturnType, true, false),
                Parameters = methodDeclaration.ParameterList.Parameters.Select(VisitMethodParameter).ToList(),
                Statements = new List<Statement> { VisitBlock(methodDeclaration.Body) }
            };

            return function;
        }

        public Block VisitBlock(BlockSyntax block) =>
            new Block
            {
                Statements = block.Statements.Select(VisitStatement).ToList()
            };

        public Statement VisitStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case BlockSyntax childBlock: return VisitBlock(childBlock);
                case LocalDeclarationStatementSyntax localDeclaration:
                {
                    var localType = VisitType(localDeclaration.Declaration.Type, true, false);
                    var localName = localDeclaration.Declaration.Variables[0].Identifier.Text;
                    Expression initializer = null;

                    var equalsValue = localDeclaration.Declaration.Variables[0].Initializer;
                    if (equalsValue != null) { initializer = VisitExpression(equalsValue.Value); }

                    return new VariableDeclaration
                    {
                        Initializer = initializer,
                        TypeName = localType,
                        VariableName = localName
                    };
                }
                case ReturnStatementSyntax returnStatement:
                    return new ReturnValueStatement
                    {
                        Expression = (returnStatement.Expression != null)
                            ? VisitExpression(returnStatement.Expression)
                            : null
                    };
                case CheckedStatementSyntax checkedStatement: return VisitBlock(checkedStatement.Block);
                case ExpressionStatementSyntax expressionStatement:
                    return new ExpressionStatement
                    {
                        Expression = VisitExpression(expressionStatement.Expression)
                    };
                case IfStatementSyntax ifStatement:
                {
                    var conditionExpression = VisitExpression(ifStatement.Condition);
                    var ifTrueStatement = VisitStatement(ifStatement.Statement);
                    var elseStatement = (ifStatement.Else != null)
                        ? VisitStatement(ifStatement.Else.Statement)
                        : null;

                    return new IfStatement
                    {
                        Condition = conditionExpression,
                        TrueStatement = ifTrueStatement,
                        ElseStatement = elseStatement
                    };
                }
                case TryStatementSyntax tryStatement:
                    return new TryCatch
                    {
                        TryStatement = VisitStatement(tryStatement.Block),
                        CatchBlocks = tryStatement.Catches.Select(c => new CatchBlock
                        {
                            CatchesExceptionType = (c.Declaration != null)
                                ? VisitType(c.Declaration.Type, true, false)
                                : "Exception*",
                            ExceptionName = (c.Declaration != null)
                                ? c.Declaration.Identifier.Text
                                : "ex",
                            Statement = VisitStatement(c.Block)
                        }).ToList(),
                        FinallyStatement = (tryStatement.Finally != null)
                            ? VisitStatement(tryStatement.Finally.Block)
                            : null
                    };
                case ThrowStatementSyntax throwStatement:
                    return new ExpressionStatement
                    {
                        Expression = new CixFunctionInvocation
                        {
                            FunctionName = "Throw",
                            Arguments = new List<Expression> { VisitExpression(throwStatement.Expression) }
                        }
                    };
                case SwitchStatementSyntax switchStatement:
                    return new SwitchBlock
                    {
                        Expression = VisitExpression(switchStatement.Expression),
                        Cases = switchStatement.Sections.Select(s => new SwitchSection
                        {
                            Labels = s.Labels.Select(VisitSwitchLabel).ToList(),
                            Body = new Block { Statements = s.Statements.Select(VisitStatement).ToList() }
                        }).ToList()
                    };
                case BreakStatementSyntax _: return new KeywordStatement { Keyword = "break" };
                case WhileStatementSyntax whileStatement:
                    return new WhileStatement
                    {
                        Statement = VisitStatement(whileStatement.Statement),
                        Expression = VisitExpression(whileStatement.Condition)
                    };

                default: throw new ArgumentException($"{statement.Kind()}");
            }
        }

        public SwitchLabel VisitSwitchLabel(SwitchLabelSyntax l)
        {
            switch (l)
            {
                case CaseSwitchLabelSyntax caseLabel:
                    return new SwitchLabel
                    {
                        Value =
                            (Literal)VisitExpression(caseLabel.Value)
                    };

                case DefaultSwitchLabelSyntax _: return new SwitchLabel();
                default: throw new ArgumentException(l.Kind().ToString());
            }
        }

        public Expression VisitExpression(CSharpSyntaxNode expression)
        {
            switch (expression)
            {
                case BinaryExpressionSyntax binaryExpression: return VisitBinaryExpression(binaryExpression);
                case InvocationExpressionSyntax invocation: return VisitInvocationExpression(invocation);
                case MemberAccessExpressionSyntax memberAccess: return VisitMemberAccessExpression(memberAccess);
                case IdentifierNameSyntax identifierName:
                {
                    var symbolContainingType = model.GetSymbolInfo(identifierName).Symbol.ContainingType;
                    var typeName = (symbolContainingType != null)
                        ? VisitType(symbolContainingType)
                        : identifierName.Identifier.Text;

                    return new Literal
                    {
                        LiteralText = identifierName.Identifier.Text,
                        TypeName = typeName
                    };
                }

                case LiteralExpressionSyntax literalExpression:
                    switch (literalExpression.Kind())
                    {
                        case SyntaxKind.NullLiteralExpression:
                            return new Literal { LiteralText = "null", TypeName = "void*" };
                        case SyntaxKind.NumericLiteralExpression:
                            var numericLiteral = literalExpression.Token.Text;
                            return new Literal
                            {
                                LiteralText = numericLiteral,
                                TypeName = GetTypeForNumericLiteral(numericLiteral)
                            };
                        case SyntaxKind.StringLiteralExpression:
                            return new Literal
                            {
                                LiteralText = literalExpression.Token.Text,
                                TypeName = "lpstring*"
                            };
                        case SyntaxKind.TrueLiteralExpression:
                            return new Literal { LiteralText = "1", TypeName = "int" };
                        case SyntaxKind.FalseLiteralExpression:
                            return new Literal { LiteralText = "0", TypeName = "int" };
                        default: throw new ArgumentException(literalExpression.Kind().ToString());
                    }
                case GenericNameSyntax genericName:
                {
                    var typeName = VisitType(genericName, false, true);
                    return new Literal
                    {
                        LiteralText = typeName,
                        TypeName = typeName
                    };
                }

                case PredefinedTypeSyntax predefinedType:
                {
                    var typeName = VisitType(predefinedType, true, false);
                    return new Literal
                    {
                        LiteralText = typeName,
                        TypeName = typeName
                    };
                }
                case ArrayCreationExpressionSyntax arrayCreation: return VisitArrayCreationExpression(arrayCreation);
                case RefExpressionSyntax refExpression:
                    return new UnaryExpression
                    {
                        Operand = VisitExpression(refExpression.Expression),
                        OperatorSymbol = "&"
                    };
                case DefaultExpressionSyntax defaultExpression:
                {
                    var typeSymbol = model.GetSymbolInfo(defaultExpression.Type).Symbol;

                    if (((INamedTypeSymbol)typeSymbol).TypeKind == TypeKind.Class)
                    {
                        return new Literal
                        {
                            LiteralText = "NULL",
                            TypeName = "int"
                        };
                    }

                    return new CixFunctionInvocation
                    {
                        FunctionName = $"DefautOf{VisitType(defaultExpression.Type, false, true)}",
                        Arguments = new List<Expression>()
                    };
                }
                case ObjectCreationExpressionSyntax objectCreation: return VisitObjectCreationExpression(objectCreation);
                case ThisExpressionSyntax thisExpression:
                    return new Literal
                    {
                        LiteralText = "this",
                        TypeName = VisitType(model.GetSymbolInfo(thisExpression).Symbol.ContainingType)
                    };

                case PrefixUnaryExpressionSyntax unaryExpression:
                    return new UnaryExpression
                    {
                        Operand = VisitExpression(unaryExpression.Operand),
                        OperatorSymbol = unaryExpression.OperatorToken.Text
                    };
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    return new ParenthesizedExpression
                    {
                        Expression = VisitExpression(parenthesizedExpression.Expression)
                    };
                case ElementAccessExpressionSyntax accessExpression:
                    return new IndexerAccess
                    {
                        Expression = VisitExpression(accessExpression.Expression),
                        Arguments = accessExpression.ArgumentList.Arguments.Select(arg => VisitExpression(arg.Expression)).ToList(),
                        TypeName = VisitType(model.GetSymbolInfo(accessExpression.Expression).Symbol.ContainingType)
                    };
                case AssignmentExpressionSyntax assignment:
                    return new BinaryExpression
                    {
                        Left = VisitExpression(assignment.Left),
                        Right = VisitExpression(assignment.Right),
                        OperatorSymbol = assignment.OperatorToken.Text
                    };
                case CastExpressionSyntax castExpression:
                {
                    var expressionType = model.GetTypeInfo(castExpression.Expression).Type;
                    return new CixFunctionInvocation
                    {
                        FunctionName = $"Cast{VisitType(expressionType)}To{VisitType(castExpression.Type, false, true)}",
                        Arguments = new List<Expression> { VisitExpression(castExpression.Expression) }
                    };
                }

                case ConditionalExpressionSyntax conditionalExpression:
                {
                    var expressionType = model.GetSymbolInfo(conditionalExpression.WhenTrue).Symbol.ContainingType;
                    return new CixFunctionInvocation
                    {
                        FunctionName = $"If_{VisitType(expressionType)}",
                        Arguments = new List<Expression>
                        {
                            VisitExpression(conditionalExpression.Condition),
                            VisitExpression(conditionalExpression.WhenTrue),
                            VisitExpression(conditionalExpression.WhenFalse)
                        }
                    };
                }

                default: throw new ArgumentException(expression.Kind().ToString());
            }
        }

        public string GetTypeForNumericLiteral(string numericLiteral)
        {
            if (numericLiteral.Contains(".")) { return "double"; }

            if (!long.TryParse(numericLiteral, out var number))
            {
                if (!ulong.TryParse(numericLiteral, out _)) { throw new ArgumentOutOfRangeException(); }

                return "ulong";
            }

            if (number < int.MinValue || number > int.MaxValue) { return "long"; }

            return "int";
        }

        public Expression VisitObjectCreationExpression(ObjectCreationExpressionSyntax objectCreation)
        {
            var typeName = VisitType(objectCreation.Type, false, true);
            var arguments = objectCreation.ArgumentList.Arguments.Select(arg => VisitExpression(arg.Expression));
            var mallocExpression = new CixFunctionInvocation
            {
                FunctionName = "malloc",
                Arguments = new List<Expression>
                {
                    new CixFunctionInvocation
                    {
                        FunctionName = "sizeof",
                        Arguments = new List<Expression>
                        {
                            new Literal { LiteralText = typeName }
                        }
                    }
                }
            };

            return new ClassStaticFunctionInvocation
            {
                TypeName = typeName,
                FunctionName = "ctor",
                Arguments = arguments.Prepend(mallocExpression).ToList()
            };
        }

        public Expression VisitArrayCreationExpression(ArrayCreationExpressionSyntax arrayCreation)
        {
            var arrayType = $"ArrayOf{VisitType(arrayCreation.Type.ElementType, false, true)}";
            var size = VisitExpression(arrayCreation.Type.RankSpecifiers[0].Sizes[0]);

            return new CixFunctionInvocation
            {
                FunctionName = $"{arrayType}_ctor",
                Arguments = new List<Expression>
                {
                    new CixFunctionInvocation
                    {
                        FunctionName = "malloc",
                        Arguments = new List<Expression>
                        {
                            new CixFunctionInvocation
                            {
                                FunctionName = "sizeof",
                                Arguments = new List<Expression>
                                {
                                    new Literal
                                    {
                                        LiteralText = arrayType,
                                        TypeName = arrayType
                                    }
                                }
                            }
                        }
                    },
                    size
                }
            };
        }

        public Expression VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            var cixExpression = new BinaryExpression
            {
                Left = VisitExpression(binaryExpression.Left),
                Right = VisitExpression(binaryExpression.Right),
                OperatorSymbol = binaryExpression.OperatorToken.Text
            };

            if (cixExpression.OperatorSymbol == "as")
            {
                var leftType = model.GetTypeInfo(binaryExpression.Left).Type.Name;
                var rightType = model.GetTypeInfo(binaryExpression.Right).Type.Name;

                return new CixFunctionInvocation
                {
                    Arguments = new List<Expression> { cixExpression.Left },
                    FunctionName = $"{leftType}To{rightType}"
                };
            }

            return cixExpression;
        }

        public Expression VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
        {
            var leftExpression = VisitExpression(memberAccess.Expression);

            var memberSymbol = model.GetSymbolInfo(memberAccess).Symbol;

            switch (memberSymbol.Kind)
            {
                case SymbolKind.Field:
                {
                    return new BinaryExpression
                    {
                        Left = leftExpression,
                        OperatorSymbol = "->",
                        Right = new Literal
                        {
                            LiteralText = memberAccess.Name.Identifier.Text,
                            TypeName = VisitType(memberSymbol.ContainingType)
                        }
                    };
                }
                case SymbolKind.Property:
                    return new PropertyAccess
                    {
                        LeftExpression = leftExpression,
                        PropertyName = memberAccess.Name.Identifier.Text,
                        IsStatic = memberSymbol.IsStatic,
                        TypeName = VisitType(memberSymbol.ContainingType)
                    };
                case SymbolKind.Method when memberSymbol.IsStatic:
                    return new ClassStaticFunctionInvocation
                    {
                        TypeName = VisitType(memberSymbol.ContainingType),
                        FunctionName = memberSymbol.Name
                    };
                case SymbolKind.Method:
                    return new ClassVirtualFunctionInvocation
                    {
                        FunctionName = memberSymbol.Name,
                        TypeName = VisitType(memberSymbol.ContainingType),
                        ThisArgument = leftExpression
                    };
                default: throw new ArgumentException(memberSymbol.Kind.ToString());
            }
        }

        public Expression VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            var invocationExpression = VisitExpression(invocation.Expression);
            var arguments = invocation.ArgumentList.Arguments.Select(arg => VisitExpression(arg.Expression));

            if (invocationExpression is ClassStaticFunctionInvocation staticFunctionInvocation)
            {
                staticFunctionInvocation.Arguments = arguments.ToList();
            }
            else if (invocationExpression is ClassVirtualFunctionInvocation virtualFunctionInvocation)
            {
                virtualFunctionInvocation.Arguments = arguments.ToList();
            }

            return invocationExpression;
        }

        public MethodParameter VisitMethodParameter(ParameterSyntax parameter) =>
            new MethodParameter
            {
                Name = parameter.Identifier.Text,
                TypeName = VisitType(parameter.Type, true, false)
            };

        public void VisitInterface(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var interfaceName = interfaceDeclaration.Identifier.Text;
            var vtable = new ClassVTableStructDefinition
            {
                Name = interfaceName + "_vtable_struct",
                Pointers = interfaceDeclaration.Members.Select(m => VisitInterfaceMember(m, interfaceName)).SelectMany(p => p).ToList()
            };

            interfaces.Add(vtable);
        }

        public List<ClassVTableFunctionPointer> VisitInterfaceMember(MemberDeclarationSyntax member, string interfaceName)
        {
            if (member is MethodDeclarationSyntax method)
            {
                return new List<ClassVTableFunctionPointer>
                {
                    VisitMethodForVTable(method, interfaceName)
                };
            }

            if (member is PropertyDeclarationSyntax property)
            {
                var getter = property.AccessorList.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);
                var setter = property.AccessorList.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.SetAccessorDeclaration);
                var accessorFunctions = new List<ClassVTableFunctionPointer>();
                var propertyTypeName = VisitType(property.Type, true, false);

                if (getter != null)
                {
                    accessorFunctions.Add(new ClassVTableFunctionPointer
                    {
                        FunctionName = $"{property.Identifier.Text}_get",
                        ParameterTypeNames = new List<string> { interfaceName },
                        ReturnTypeName = propertyTypeName
                    });
                }

                if (setter != null)
                {
                    accessorFunctions.Add(new ClassVTableFunctionPointer
                    {
                        FunctionName = $"{property.Identifier.Text}_set",
                        ParameterTypeNames = new List<string> { interfaceName, propertyTypeName },
                        ReturnTypeName = "void"
                    });
                }

                return accessorFunctions;
            }

            throw new ArgumentException(member.Kind().ToString());
        }

        public void VisitEnum(EnumDeclarationSyntax enumDeclaration)
        {
            var enumName = enumDeclaration.Identifier.Text;
            var lastSeenValue = -1;

            foreach (var member in enumDeclaration.Members)
            {
                if (member.EqualsValue != null)
                {
                    var valueExpression = VisitExpression(member.EqualsValue.Value);
                    if (!(valueExpression is Literal)) { throw new ArgumentException(member.EqualsValue.Value.Kind().ToString()); }

                    var value = int.Parse(((Literal)valueExpression).LiteralText);
                    lastSeenValue = value;
                }

                enumDefines.Add($"{enumName.ToUpperInvariant()}_{member.Identifier.Text.ToUpperInvariant()}", lastSeenValue);
                lastSeenValue++;
            }
        }

        public Statement VisitStaticField(FieldDeclarationSyntax fieldDeclaration)
        {
            var variable = fieldDeclaration.Declaration.Variables[0];
            var type = VisitType(fieldDeclaration.Declaration.Type, false, true);
            var name = variable.Identifier.Text;

            return new VariableDeclaration
            {
                Initializer = (variable.Initializer != null)
                    ? VisitExpression(variable.Initializer.Value)
                    : null,
                TypeName = type,
                VariableName = name
            };
        }
    }
}
