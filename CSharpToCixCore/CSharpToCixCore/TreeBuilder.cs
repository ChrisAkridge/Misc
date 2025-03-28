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

namespace CSharpToCixCore
{
	internal sealed class TreeBuilder
    {
        private readonly Random random = new Random();
        private readonly List<ClassTransform> classTransforms = new List<ClassTransform>();
        private readonly List<VariableDeclaration> globals = new List<VariableDeclaration>();
        private readonly List<Statement> globalInitializerStatements = new List<Statement>();
        private readonly List<ClassVTableStructDefinition> interfaces = new List<ClassVTableStructDefinition>();
        private readonly List<StaticMethodFunction> additionalFunctions = new List<StaticMethodFunction>();
        private readonly List<CixStruct> anonymousObjectStructs = new List<CixStruct>();
        private readonly Dictionary<string, int> enumDefines = new Dictionary<string, int>();
        private string namespaceName;
        private SemanticModel model;
        private int lambdaFunctionIndex;

        public IReadOnlyList<ClassTransform> ClassTransforms => classTransforms.AsReadOnly();
        public IReadOnlyList<VariableDeclaration> Globals => globals.AsReadOnly();
        public IReadOnlyList<Statement> GlobalInitializerStatements => globalInitializerStatements.AsReadOnly();
        public IReadOnlyList<ClassVTableStructDefinition> Interfaces => interfaces.AsReadOnly();
        public IReadOnlyList<StaticMethodFunction> AdditionalFunctions => additionalFunctions.AsReadOnly();
        public IReadOnlyList<CixStruct> AnonymousObjectStructs => anonymousObjectStructs.AsReadOnly();
        public IReadOnlyDictionary<string, int> EnumDefines => new ReadOnlyDictionary<string, int>(enumDefines);

        public void BuildTree(SyntaxTree syntaxTree, SemanticModel model)
		{
			var root = syntaxTree.GetCompilationUnitRoot();

			 //var diagnostics = compilation.GetDiagnostics();
			 //if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) { throw new Exception("Errors were present in the source code."); }

            this.model = model;

			VisitMembers(root.Members);
		}

		private void VisitMembers(SyntaxList<MemberDeclarationSyntax> members)
		{
			foreach (var node in members)
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

					case NamespaceDeclarationSyntax namespaceDeclaration:
						VisitNamespace(namespaceDeclaration);
						break;
				}
			}
		}

		public void VisitNamespace(NamespaceDeclarationSyntax namespaceDeclaration)
        {
            namespaceName = namespaceDeclaration.Name.GetText().ToString().Replace(".", "_").Trim();
            VisitMembers(namespaceDeclaration.Members);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void VisitClass(TypeDeclarationSyntax classDeclaration, string parentClassName = "")
        {
            if (classDeclaration.Arity > 0) { return; }

            var classSymbol = model.GetDeclaredSymbol(classDeclaration);

            var className = (parentClassName == string.Empty)
                ? classDeclaration.Identifier.Text
                : parentClassName + "_" + classDeclaration.Identifier.Text;

            if (className.Contains("JobManager"))
            {
                System.Diagnostics.Debugger.Break();
            }

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

            var statements = (methodDeclaration.Body != null)
                ? new List<Statement> { VisitBlock(methodDeclaration.Body) }
                : new List<Statement>
                {
                    new ExpressionStatement
                    {
                        Expression = VisitExpression(methodDeclaration.ExpressionBody)
                    }
                };

            return new StaticMethodFunction
            {
                MethodOf = VisitType(containingType),
                Name = methodDeclaration.Identifier.Text,
                Parameters = methodDeclaration.ParameterList.Parameters.Select(VisitMethodParameter).ToList(),
                ReturnTypeName = VisitType(methodDeclaration.ReturnType, true, false),
                Statements = statements
            };
        }

        public StaticMethodFunction VisitLocalFunction(LocalFunctionStatementSyntax localFunctionStatement)
        {
            var statements = (localFunctionStatement.Body != null)
                ? new List<Statement> { VisitBlock(localFunctionStatement.Body) }
                : new List<Statement>
                {
                    new ExpressionStatement
                    {
                        Expression = VisitExpression(localFunctionStatement.ExpressionBody)
                    }
                };

            return new StaticMethodFunction
            {
                MethodOf = "LocalFunctions",
                Name = localFunctionStatement.Identifier.Text,
                Parameters = localFunctionStatement.ParameterList.Parameters.Select(VisitMethodParameter).ToList(),
                ReturnTypeName = VisitType(localFunctionStatement.ReturnType, true, false),
                Statements = statements
            };
        }

        public StaticMethodFunction[] VisitStaticProperty(PropertyDeclarationSyntax prop)
        {
            var containingType = model.GetDeclaredSymbol(prop).ContainingType;
            var returnType = prop.Type;
            var propertyName = prop.Identifier.Text;
            var returnTypeName = VisitType(returnType, true, false);

            if (prop.ExpressionBody != null)
            {
                var expressionGetterFunction = new StaticMethodFunction
                {
                    MethodOf = VisitType(containingType),
                    Name = $"{prop.Identifier.Text}_get",
                    Parameters = new List<MethodParameter>(),
                    ReturnTypeName = returnTypeName,
                    Statements = new List<Statement>
                    {
                        new ExpressionStatement { Expression = VisitExpression(prop.ExpressionBody) }
                    }
                };

                return new[] { expressionGetterFunction };
            }

            var getter = prop.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            var setter = prop.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);

            StaticMethodFunction getterFunction = null;
            StaticMethodFunction setterFunction = null;

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
            var statements = (constructorDeclaration.Body != null)
                ? new List<Statement> { VisitBlock(constructorDeclaration.Body) }
                : new List<Statement>
                {
                    new ExpressionStatement
                    {
                        Expression = VisitExpression(constructorDeclaration.ExpressionBody)
                    }
                };

            return new StaticMethodFunction
            {
                MethodOf = containingTypeName,
                Name = "ctor",
                Parameters = constructorDeclaration.ParameterList.Parameters.Select(VisitMethodParameter)
                    .Prepend(thisParameter).ToList(),
                Statements = statements
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
                case NullableTypeSyntax nullableType:
                    return $"NullableOf{VisitType(nullableType.ElementType, false, true)}";
                case TupleTypeSyntax tupleTypeSyntax:
                    return $"ValueTupleOf{string.Concat(tupleTypeSyntax.Elements.Select(t => t.Identifier.Text))}";
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
            var propertyTypeName = VisitType(returnType, true, false);

            if (propertyDeclaration.AccessorList == null)
            {
                var getExpression = VisitExpression(propertyDeclaration.ExpressionBody);
                return new[]
                {
                    new InstanceMethodFunction
                    {
                        MethodOf = VisitType(containingType),
                        Name = $"{propertyDeclaration.Identifier.Text}_get",
                        Parameters = new List<MethodParameter>
                        {
                            new MethodParameter { Name = "this", TypeName = VisitType(containingType) + "*" }
                        },
                        ReturnTypeName = propertyTypeName,
                        Statements = new List<Statement>
                        {
                            new ExpressionStatement
                            {
                                Expression = getExpression
                            }
						}
                    }
                };
            }

            var getter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.GetAccessorDeclaration);
            var setter = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(acc => acc.Kind() == SyntaxKind.SetAccessorDeclaration);

            InstanceMethodFunction getterFunction = null;
            InstanceMethodFunction setterFunction = null;

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
            var statements = (methodDeclaration.Body != null)
                ? new List<Statement> { VisitBlock(methodDeclaration.Body) }
                : (methodDeclaration.ExpressionBody != null)
                    ? new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = VisitExpression(methodDeclaration.ExpressionBody)
                        }
                    }
                    : new List<Statement>();

            var function = new InstanceMethodFunction
            {
                MethodOf = className,
                Name = methodDeclaration.Identifier.Text,
                ReturnTypeName = VisitType(methodDeclaration.ReturnType, true, false),
                Parameters = methodDeclaration.ParameterList.Parameters.Select(VisitMethodParameter).ToList(),
                Statements = statements
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
                case YieldStatementSyntax yieldStatement:
                    return new ReturnValueStatement
                    {
                        Expression = VisitExpression(yieldStatement.Expression)
                    };
                case CheckedStatementSyntax checkedStatement: return VisitBlock(checkedStatement.Block);
				case ExpressionStatementSyntax expressionStatement:
					return VisitExpressionStatement(expressionStatement);
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
                {
                    if (throwStatement.Expression != null)
                    {
                        return new ExpressionStatement
                        {
                            Expression = new CixFunctionInvocation
                            {
                                FunctionName = "Throw",
                                Arguments = new List<Expression> { VisitExpression(throwStatement.Expression) }
                            }
                        };
                    }

                    return new ExpressionStatement
                    {
                        Expression = new CixFunctionInvocation
                        {
                            FunctionName = "Throw",
                            Arguments = new List<Expression>()
                        }
                    };
                }
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
                case ForStatementSyntax forStatement:
                    return VisitForStatement(forStatement);
                case ForEachStatementSyntax forEachStatement:
                    return VisitForEachStatement(forEachStatement);
                case ContinueStatementSyntax _:
                    return new ExpressionStatement
                    {
                        Expression = new Literal { LiteralText = "continue" }
                    };
                case DoStatementSyntax doStatement:
                {
                    return new DoWhileStatement
                    {
                        Statement = VisitStatement(doStatement.Statement),
                        Expression = VisitExpression(doStatement.Condition)
                    };
                }
                case ForEachVariableStatementSyntax forEachVariableStatement:
                {
                    return VisitForEachVariableStatement(forEachVariableStatement);
                }
                case UsingStatementSyntax usingStatement: { return VisitUsingStatement(usingStatement); }
                case LocalFunctionStatementSyntax localFunctionStatement:
                {
                    additionalFunctions.Add(VisitLocalFunction(localFunctionStatement));
                    return new ExpressionStatement
                    {
                        Expression = new Literal
                        {
                            LiteralText = "\"Lifted local function from here.\"",
                            TypeName = "byte*"
                        }
                    };
                }

                case EmptyStatementSyntax emptyStatement:
                {
                    return new StatementList
                    {
                        Statements = new List<Statement>()
                    };
                }

                default: throw new ArgumentException($"{statement.Kind()}");
            }
        }

        private Statement VisitUsingStatement(UsingStatementSyntax usingStatement)
        {
            // using (T x = y) statement; becomes
            //
            // T x = y;
            // { statement; }
            // x.Dispose();

            var variableDeclarations = usingStatement.Declaration.Variables
                .Select(v => new VariableDeclaration
                {
                    TypeName = VisitType(model.GetDeclaredSymbol(v).ContainingType),
                    VariableName = v.Identifier.Text,
                    Initializer = VisitExpression(v.Initializer)
                });

            var disposalStatements = variableDeclarations
                .Select(v => new ExpressionStatement
                {
                    Expression = new BinaryExpression
                    {
                        Left = new Literal
                        {
                            LiteralText = v.VariableName,
                            TypeName = v.TypeName
                        },
                        Right = new CixFunctionInvocation
                        {
                            FunctionName = $"{v.TypeName}_Dispose",
                            Arguments = new List<Expression>()
                        },
                        OperatorSymbol = "."
                    }
                });

            return new StatementList
            {
                Statements = variableDeclarations
                    .Append(VisitStatement(usingStatement.Statement))
                    .Concat(disposalStatements)
                    .ToList()
            };
        }

        private Statement VisitForEachVariableStatement(ForEachVariableStatementSyntax forEachVariableStatement)
        {
            // foreach (T (a, b, c) in Y) statement; becomes
            //
            // IEnumeratorOfT_vtable_struct __enumerator_randomNumber = typeOfY_GetEnumerator(y);
            // TA a;
            // TB b;
            // TC c;
            // while (__enumerator_randomNumber.MoveNext()) {
            //  T current = __enumerator.randomNumber.Current_get();
            //  a = deconstructed.a;
            //  b = deconstructed.b;
            //  c = deconstructed.c;
            //  statement;
            // }
            // __enumerator_randomNumber.Dispose();

            var enumeratorId = random.Next();
            var enumeratorExpression = forEachVariableStatement.Expression;
            var enumerationType = VisitType(model.GetTypeInfo(enumeratorExpression).Type);
            var enumeratedType = VisitType(model.GetTypeInfo(forEachVariableStatement.Variable).Type);

            AddEnumeratorInterfaceIfNotPresent(enumeratedType);

			var declarationExpression = forEachVariableStatement.Variable as DeclarationExpressionSyntax;
			var parenthesizedVariableDesignation =
				(ParenthesizedVariableDesignationSyntax)declarationExpression?.Designation;
			var tupleExpression = forEachVariableStatement.Variable as TupleExpressionSyntax;
            var arguments = tupleExpression?.Arguments;

            var enumeratorDeclaration = new VariableDeclaration
            {
                TypeName = $"IEnumeratorOf{enumeratedType}_vtable_struct",
                VariableName = $"__enumerator_{enumeratorId}",
                Initializer = new CixFunctionInvocation
                {
                    FunctionName = $"{enumerationType}_GetEnumerator",
                    Arguments = new List<Expression>
                    {
                        VisitExpression(enumeratorExpression)
                    }
                }
            };

            IEnumerable<VariableDeclaration> declarations;

            if (parenthesizedVariableDesignation != null)
            {
                declarations = parenthesizedVariableDesignation.Variables
                    .Select(v => new VariableDeclaration
                    {
                        TypeName = VisitType(model.GetTypeInfo(v).Type),
                        VariableName = v.GetText().ToString()
                    });
            }
            else
            {
                declarations = arguments.Value
                    .Select(arg =>
                    {
                        var declaration = (DeclarationExpressionSyntax)arg.Expression;
                        return new VariableDeclaration
                        {
                            TypeName = VisitType(declaration.Type, true, false),
                            VariableName = declaration.Designation.GetText().ToString()
                        };
                    });
            }
            
            var currentGet = new VariableDeclaration
            {
                TypeName = enumeratedType,
                VariableName = "current",
                Initializer = new BinaryExpression
                {
                    Left = new Literal
                    {
                        LiteralText = $"__enumerator_{enumeratorId}",
                        TypeName = $"IEnumeratorOf{enumeratedType}"
                    },
                    Right = new CixFunctionInvocation
                    {
                        FunctionName = "Current_get",
                        Arguments = new List<Expression>()
                    },
                    OperatorSymbol = "."
                }
            };

            var assignments = declarations.Select(d => new ExpressionStatement
            {
                Expression = new BinaryExpression
                {
                    Left = new Literal
                    {
                        LiteralText = d.VariableName,
                        TypeName = d.TypeName
                    },
                    Right = new BinaryExpression
                    {
                        Left = new Literal
                        {
                            LiteralText = "Current",
                            TypeName = enumeratedType
                        },
                        Right = new Literal
                        {
                            LiteralText = d.VariableName,
                            TypeName = d.TypeName
                        },
                        OperatorSymbol = "."
                    },
                    OperatorSymbol = "="
                }
            });

            var whileLoopStatements = new List<Statement>();
            whileLoopStatements.Add(currentGet);
            whileLoopStatements.AddRange(assignments);
            whileLoopStatements.Add(VisitStatement(forEachVariableStatement.Statement));

            var whileLoop = new WhileStatement
            {
                Expression = new BinaryExpression
                {
                    Left = new Literal
                    {
                        LiteralText = $"__enumerator_{enumeratorId}",
                        TypeName = $"IEnumeratorOf{enumeratedType}"
                    },
                    Right = new CixFunctionInvocation
                    {
                        FunctionName = "MoveNext",
                        Arguments = new List<Expression>()
                    },
                    OperatorSymbol = "."
                },
                Statement = new Block
                {
                    Statements = whileLoopStatements
                }
            };

            var dispose = new ExpressionStatement
            {
                Expression = new BinaryExpression
                {
                    Left = new Literal
                    {
                        LiteralText = $"__enumerator_{enumeratorId}",
                        TypeName = $"IEnumeratorOf{enumeratedType}"
                    },
                    Right = new CixFunctionInvocation
                    {
                        FunctionName = "Dispose",
                        Arguments = new List<Expression>()
                    },
                    OperatorSymbol = "."
                }
            };

            var blockStatements = new List<Statement>();
            blockStatements.Add(enumeratorDeclaration);
            blockStatements.AddRange(declarations);
            blockStatements.Add(whileLoop);
            blockStatements.Add(dispose);

            return new Block
            {
                Statements = blockStatements
            };
        }

        private StatementList VisitForEachStatement(ForEachStatementSyntax forEachStatement)
        {
            // foreach (T x in y) statement; becomes
            //
            // IEnumeratorOfT_vtable_struct __enumerator_randomNumber = typeOfY_GetEnumerator(y);
            // while (__enumerator_randomNumber.MoveNext()) {
            //  T x = __enumerator_randomNumber.Current_get();
            //  statement;
            // }
            // __enumerator_randomNumber.Dispose();

            var enumeratorId = random.Next();
            var enumeratorExpression = forEachStatement.Expression;
            var enumerationType = VisitType(model.GetTypeInfo(enumeratorExpression).Type);
            var enumeratedType = VisitType(model.GetTypeInfo(forEachStatement.Type).Type);

            AddEnumeratorInterfaceIfNotPresent(enumeratedType);

            var declaration = new VariableDeclaration
            {
                TypeName = $"IEnumeratorOf{enumeratedType}_vtable_struct",
                VariableName = $"__enumerator_{enumeratorId}",
                Initializer = new CixFunctionInvocation
                {
                    FunctionName = $"{enumerationType}_GetEnumerator",
                    Arguments = new List<Expression> { VisitExpression(enumeratorExpression) }
                }
            };

            var currentDeclaration = new VariableDeclaration
            {
                TypeName = enumeratedType,
                VariableName = forEachStatement.Identifier.Text,
                Initializer = new BinaryExpression
                {
                    Left = new Literal { LiteralText = $"__enumerator_{enumeratorId}" },
                    OperatorSymbol = ".",
                    Right = new CixFunctionInvocation
                    {
                        FunctionName = "Current_get",
                        Arguments = new List<Expression>()
                    }
                }
            };

            var whileBlock = new WhileStatement
            {
                Expression = new BinaryExpression
                {
                    Left = new Literal { LiteralText = $"__enumerator_{enumeratorId}" },
                    OperatorSymbol = ".",
                    Right = new CixFunctionInvocation
                    {
                        FunctionName = "MoveNext",
                        Arguments = new List<Expression>()
                    }
                },
                Statement = new StatementList
                {
                    Statements = new List<Statement>
                    {
                        currentDeclaration,
                        VisitStatement(forEachStatement.Statement)
                    }
				}
            };

            return new StatementList
            {
                Statements = new List<Statement>
                {
                    declaration,
                    whileBlock
                }
            };
        }

        private void AddEnumeratorInterfaceIfNotPresent(string enumeratedType)
        {
            if (interfaces.All(i => i.Name != $"IEnumeratorOf{enumeratedType}"))
            {
                interfaces.Add(new ClassVTableStructDefinition
                {
                    Name = $"IEnumeratorOf{enumeratedType}_struct",
                    Pointers = new List<ClassVTableFunctionPointer>
                    {
                        new ClassVTableFunctionPointer
                        {
                            FunctionName = "MoveNext",
                            ParameterTypeNames = new List<string>(),
                            ReturnTypeName = "int"
                        },
                        new ClassVTableFunctionPointer
                        {
                            FunctionName = "Current_get",
                            ParameterTypeNames = new List<string>(),
                            ReturnTypeName = enumeratedType
                        }
                    }
                });
            }
        }

        private Statement VisitExpressionStatement(ExpressionStatementSyntax expressionStatement)
		{
            if (expressionStatement.Expression is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.Left is DeclarationExpressionSyntax declarationExpression
                    && declarationExpression.Designation is ParenthesizedVariableDesignationSyntax
                        parenthesizedVariableDesignation)
                {
                    var declarations = parenthesizedVariableDesignation.Variables
                        .Select(v => new VariableDeclaration
                        {
                            TypeName = "void*",
                            VariableName = v.GetText().ToString()
                        })
                        .ToList();

                    var expressionType = model.GetTypeInfo(assignmentExpression.Right).Type;
                    var assignment = new CixFunctionInvocation
                    {
                        FunctionName = $"{VisitType(expressionType)}_Deconstruct",
                        Arguments = new List<Expression>
                        {
                            VisitExpression(assignmentExpression.Right)
                        }
                    };

                    assignment.Arguments.AddRange(declarations.Select(d => new Literal
                    {
                        LiteralText = d.VariableName,
                        TypeName = d.TypeName
                    }));

                    var statementList = new StatementList { Statements = new List<Statement>() };
                    statementList.Statements.AddRange(declarations);
                    statementList.Statements.Add(new ExpressionStatement { Expression = assignment });
                    return statementList;
                }
                
                if (assignmentExpression.Left is TupleExpressionSyntax tupleExpression)
                {
                    var declarations = tupleExpression.Arguments
                        .Select(arg =>
                        {
                            var declaration = (DeclarationExpressionSyntax)arg.Expression;
                            return new VariableDeclaration
                            {
                                VariableName = declaration.Designation.GetText().ToString(),
                                TypeName = VisitType(declaration.Type, true, false)
                            };
                        });

                    return new StatementList
                    {
                        Statements = declarations.Cast<Statement>().ToList()
                    };
                }
            }

			return new ExpressionStatement
			{
				Expression = VisitExpression(expressionStatement.Expression)
			};
		}

		public Block VisitForStatement(ForStatementSyntax forStatement)
        {
            var declarations = (forStatement.Declaration != null)
                ? VisitVariableDeclarationSyntax(forStatement.Declaration)
                : null;

            var initializers = forStatement.Initializers
                .Select(i => new ExpressionStatement { Expression = VisitExpression(i) });

            var condition = VisitExpression(forStatement.Condition);
            var statement = VisitStatement(forStatement.Statement);

            var whileStatementBlock = new Block { Statements = new List<Statement>() };
            if (declarations != null) { whileStatementBlock.Statements.AddRange(declarations); }
            whileStatementBlock.Statements.AddRange(initializers);
            whileStatementBlock.Statements.Add(new WhileStatement
                {
                    Expression = condition,
                    Statement = statement
                });

            return whileStatementBlock;
        }

        public IEnumerable<VariableDeclaration> VisitVariableDeclarationSyntax(VariableDeclarationSyntax variableDeclaration)
        {
            foreach (var variable in variableDeclaration.Variables)
            {
                yield return new VariableDeclaration
                {
                    TypeName = VisitType((ITypeSymbol)model.GetSymbolInfo(variableDeclaration.Type).Symbol),
                    Initializer = VisitExpression(variable.Initializer),
                    VariableName = variable.Identifier.Text
                };
            }
        }

        public SwitchLabel VisitSwitchLabel(SwitchLabelSyntax l)
        {
            switch (l)
            {
                case CaseSwitchLabelSyntax caseLabel:
                    var labelExpression = VisitExpression(caseLabel.Value);
                    switch (labelExpression)
                    {
                        case Literal literal:
                            return new SwitchLabel
                            {
                                Value = literal
                            };

                        case BinaryExpression binaryExpression:
                        {
                            if (binaryExpression.Left is Literal left && binaryExpression.Right is Literal right)
                            {
                                return new SwitchLabel
                                {
                                    Value = new Literal
                                    {
                                        LiteralText =
                                            $"{left.LiteralText.ToUpperInvariant()}_{right.LiteralText.ToUpperInvariant()}"
                                    }
                                };
                            }

                            break;
                        }
                    }
                    throw new NotImplementedException($"{caseLabel.Value.Kind()}");

                case DefaultSwitchLabelSyntax _: return new SwitchLabel();
                case CasePatternSwitchLabelSyntax casePatternSwitchLabel:
                    // this is where it all goes off the rails
                    // because we'd have to rewrite this into an if block if we were to do it "right"
                    return new SwitchLabel
                    {
                        Value = new Literal
                        {
                            LiteralText = $"\"{casePatternSwitchLabel.GetHashCode()}\""
                        }
                    };
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
                case IdentifierNameSyntax identifierName: { return VisitIdentifierName(identifierName); }

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
                        case SyntaxKind.CharacterLiteralExpression:
                            return new Literal
                            {
                                LiteralText = ((int)literalExpression.Token.Text[0]).ToString(),
                                TypeName = "byte"
                            };
                        case SyntaxKind.TrueLiteralExpression:
                            return new Literal { LiteralText = "1", TypeName = "int" };
                        case SyntaxKind.FalseLiteralExpression:
                            return new Literal { LiteralText = "0", TypeName = "int" };
                        case SyntaxKind.DefaultLiteralExpression:
                            return new CixFunctionInvocation
                            {
                                FunctionName = "Default",
                                Arguments = new List<Expression>()
                            };
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
                    return new PrefixUnaryExpression
                    {
                        Operand = VisitExpression(refExpression.Expression),
                        OperatorSymbol = "&"
                    };
                case DefaultExpressionSyntax defaultExpression:
                {
                    var typeSymbol = model.GetSymbolInfo(defaultExpression.Type).Symbol;

                    if (typeSymbol is ITypeParameterSymbol typeParameter)
                    {
                        return new CixFunctionInvocation
                        {
                            FunctionName = $"DefautOf{typeParameter.Name}",
                            Arguments = new List<Expression>()
                        };
                    }

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
                    return new PrefixUnaryExpression
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
					return VisitAssignmentExpression(assignment);
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
                    var expressionType = model.GetTypeInfo(expression).Type;
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

                case AwaitExpressionSyntax awaitExpression: { return VisitExpression(awaitExpression.Expression); }

                case ArrowExpressionClauseSyntax arrowExpression: { return VisitExpression(arrowExpression.Expression); }

                case EqualsValueClauseSyntax equalsValueClause: { return VisitExpression(equalsValueClause.Value); }

                case ConditionalAccessExpressionSyntax conditionalAccessExpression:
                {
                    return VisitConditionalAccessExpression(conditionalAccessExpression);
                }

                case MemberBindingExpressionSyntax memberBindingExpression:
                {
                    return new Literal
                    {
                        LiteralText = memberBindingExpression.Name.Identifier.Text,
                        TypeName = "@funcptr"
                    };
                }

                case InterpolatedStringExpressionSyntax interpolatedStringExpression:
                {
                    return VisitInterpolatedStringExpression(interpolatedStringExpression);
                }

                case SimpleLambdaExpressionSyntax simpleLambdaExpression:
                {
                    return VisitSimpleLambdaExpression(simpleLambdaExpression);
                }

                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression:
                {
                    return VisitParenthesizedLambdaExpression(parenthesizedLambdaExpression);
                }

                case PostfixUnaryExpressionSyntax postfixUnaryExpression:
                {
                    return new PostfixUnaryExpression
                    {
                        Operand = VisitExpression(postfixUnaryExpression.Operand),
                        OperatorSymbol = postfixUnaryExpression.OperatorToken.Text
                    };
                }

                case SwitchExpressionSyntax switchExpression: { return VisitSwitchExpression(switchExpression); }

                case ThrowExpressionSyntax throwExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "Throw",
                        Arguments = new List<Expression>
                        {
                            VisitExpression(throwExpression.Expression)
                        }
                    };
                }

                case DeclarationExpressionSyntax declarationExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "malloc",
                        Arguments = new List<Expression>
                        {
                            new CixFunctionInvocation
                            {
                                FunctionName = "sizeof",
                                Arguments = new List<Expression>
                                {
                                    new Literal { LiteralText = VisitType(declarationExpression.Type, false, false) }
                                }
                            }
                        }
                    };
                }

                case SizeOfExpressionSyntax sizeOfExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "sizeof",
                        Arguments = new List<Expression>
                        {
                            new Literal
                            {
                                LiteralText = VisitType(sizeOfExpression.Type, false, false),
                                TypeName = "long"
                            }
                        }
                    };
                }

                case TypeOfExpressionSyntax typeOfExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "typeof",
                        Arguments = new List<Expression>
                        {
                            new Literal
                            {
                                LiteralText = $"\"{VisitType(typeOfExpression.Type, false, false)}\"",
                                TypeName = "long"
                            }
                        }
                    };
                }

                case InitializerExpressionSyntax initializerExpression:
                {
                    // https://www.youtube.com/watch?v=vRjqJVfx4A0
                    // "It looks like something they found on the ship at Roswell..."
                    return new Literal
                    {
                        LiteralText = "0",
                        TypeName = "int"
                    };
                }

                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpression:
                {
                    return VisitAnonymousObjectCreationExpression(anonymousObjectCreationExpression);
                }

                case BaseExpressionSyntax baseExpression:
                {
                    var baseType = model.GetTypeInfo(baseExpression).Type;
                    return new Literal
                    {
                        LiteralText = baseType.Name + "_vtable_struct",
                        TypeName = baseType.Name
                    };
                }

                case OmittedArraySizeExpressionSyntax omittedArraySizeExpression:
                {
                    return new Literal
                    {
                        LiteralText = "1024",
                        TypeName = "int"
                    };
                }

                case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "malloc",
                        Arguments = new List<Expression>
                        {
                            new Literal
                            {
                                LiteralText = "8",
                                TypeName = "int"
                            }
                        }
                    };
                }

                case TupleExpressionSyntax tupleExpression:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "CreateTuple",
                        Arguments = tupleExpression.Arguments.Select(VisitExpression).ToList()
                    };
                }

                case ArgumentSyntax argument:
                {
                    return VisitExpression(argument.Expression);
                }

                case ImplicitElementAccessSyntax implicitElementAccess:
                {
                    return new CixFunctionInvocation
                    {
                        FunctionName = "ElementAt",
                        Arguments = implicitElementAccess.ArgumentList.Arguments
                            .Select(VisitExpression)
                            .ToList()
                    };
                }

                case QualifiedNameSyntax qualifiedName:
                {
                    return new Literal
                    {
                        LiteralText = VisitQualifiedName(qualifiedName)
                    };
                }

                case ArrayTypeSyntax arrayType:
                {
                    return new Literal
                    {
                        LiteralText = $"ArrayOf{VisitType(arrayType.ElementType, false, true)}"
                    };
                }

                case NullableTypeSyntax nullableType:
                {
                    return new Literal
                    {
                        LiteralText = $"NullableOf{VisitType(nullableType.ElementType, false, true)}"
                    };
                }

                case QueryExpressionSyntax queryExpression:
                {
                    // nope.
                    return new CixFunctionInvocation
                    {
                        FunctionName = "Query",
                        Arguments = new List<Expression>()
                    };
                }

                default: throw new ArgumentException(expression.Kind().ToString());
            }
        }

        private Expression VisitAnonymousObjectCreationExpression(
            AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpression)
        {
            var objectMembers = anonymousObjectCreationExpression.Initializers
                .Select(i => new StructMember
                {
                    Name = i.NameEquals?.Name.Identifier.Text ?? $"__anonfield_{i.GetHashCode()}",
                    TypeName = VisitType(model.GetTypeInfo(i.Expression).Type)
                });

            var anonymousStructName = $"__anonstruct_{anonymousObjectCreationExpression.GetHashCode()}";
            anonymousObjectStructs.Add(new CixStruct
            {
                Name = anonymousStructName,
                Members = objectMembers.ToList()
            });

            return new CixFunctionInvocation
            {
                FunctionName = $"{anonymousStructName}_ctor",
                Arguments = anonymousObjectCreationExpression.Initializers
                    .Select(i => VisitExpression(i.Expression)).ToList()
            };
        }

        private Expression VisitIdentifierName(IdentifierNameSyntax identifierName)
        {
            if (identifierName.Identifier.Text == "nameof")
            {
                var entireNameofExpression = identifierName.Parent as InvocationExpressionSyntax;
                var itemNamed = entireNameofExpression.ArgumentList.Arguments[0].GetText().ToString();
                return new Literal
                {
                    LiteralText = itemNamed,
                };
            }

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

        private Expression VisitSwitchExpression(SwitchExpressionSyntax switchExpression)
        {
            // variable switch
            // {
            //  1 => 2,
            //  3 => 4
            // }
            //
            // becomes
            // (variable == 1) ? 2 : (variable == 3) ? 4;
            // so a lisp-ish sort of cons thing

            var conditionalFunctionCalls = new List<CixFunctionInvocation>();
            var governingExpression = VisitExpression(switchExpression.GoverningExpression);

            foreach (var arm in switchExpression.Arms)
            {
                Expression baseExpression = null;
                Expression whenExpression = null;
                switch (arm.Pattern)
                {
                    case ConstantPatternSyntax constantPattern:
                        baseExpression = VisitExpression(constantPattern.Expression);
                        break;
                    case DiscardPatternSyntax _: // something funny about using a discard in parsing a DiscardPatternSyntax
                        baseExpression = new Literal
                        {
                            LiteralText = "1",
                            TypeName = "int"
                        };

                        break;
                    default:
                        throw new NotImplementedException(
                            $"Type: {arm.Pattern.GetType().Name} Kind: {arm.Pattern.Kind()}");
                }

                if (arm.WhenClause != null)
                {
                    switch (arm.WhenClause)
                    {
                        default:
                            throw new NotImplementedException(
                                $"Type: {arm.WhenClause.GetType().Name} Kind: {arm.WhenClause.Kind()}");
                    }
                }

                if (whenExpression != null)
                {
                    baseExpression = new BinaryExpression
                    {
                        Left = baseExpression,
                        Right = whenExpression,
                        OperatorSymbol = "&&"
                    };
                }

                baseExpression = new BinaryExpression
                {
                    Left = governingExpression,
                    Right = baseExpression,
                    OperatorSymbol = "=="
                };

                conditionalFunctionCalls.Add(new CixFunctionInvocation
                {
                    FunctionName = "SwitchExpressionFunc",
                    Arguments = new List<Expression>
                    {
                        baseExpression,
                        VisitExpression(arm.Expression),
                        new Literal { LiteralText = "0", TypeName = "int" }
                    }
                });
            }

            return Enumerable.Reverse(conditionalFunctionCalls)
                .Aggregate((a, b) => new CixFunctionInvocation
                {
                    FunctionName = "SwitchExpressionFunc",
                    Arguments = new List<Expression>
                    {
                        b.Arguments[0],
                        b.Arguments[1],
                        a
                    }
                });
        }

        private Expression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax simpleLambdaExpression)
        {
            var parameterType = model.GetTypeInfo(simpleLambdaExpression.Parameter).Type;
            var returnType = model.GetTypeInfo(simpleLambdaExpression).ConvertedType;
            Block lambdaBlock = null;
            if (simpleLambdaExpression.ExpressionBody != null)
            {
                lambdaBlock = new Block
                {
                    Statements = new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = VisitExpression(simpleLambdaExpression.ExpressionBody)
                        }
                    }
                };
            }
            else if (simpleLambdaExpression.Block != null) { lambdaBlock = VisitBlock(simpleLambdaExpression.Block); }

            additionalFunctions.Add(new StaticMethodFunction
            {
                MethodOf = "Lambdas",
                Name = $"Lambda{lambdaFunctionIndex}",
                Parameters = new List<MethodParameter>
                {
                    new MethodParameter
                    {
                        Name = simpleLambdaExpression.Parameter.Identifier.Text,
                        TypeName = /* VisitType(parameterType) */ "void*" // sigh
                    }
                },
                ReturnTypeName = VisitType(returnType),
                Statements = new List<Statement> { lambdaBlock }
            });

            var functionPointerReference = new PrefixUnaryExpression
            {
                OperatorSymbol = "&",
                Operand = new Literal
                {
                    LiteralText = $"Lambdas_Lambda{lambdaFunctionIndex}"
                }
            };

            lambdaFunctionIndex += 1;
            return functionPointerReference;
        }

        private Expression VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression)
        {
            var returnType = model.GetTypeInfo(parenthesizedLambdaExpression).ConvertedType;
            Block lambdaBlock = null;
            if (parenthesizedLambdaExpression.ExpressionBody != null)
            {
                lambdaBlock = new Block
                {
                    Statements = new List<Statement>
                    {
                        new ExpressionStatement
                        {
                            Expression = VisitExpression(parenthesizedLambdaExpression.ExpressionBody)
                        }
                    }
                };
            }
            else if (parenthesizedLambdaExpression.Block != null) { lambdaBlock = VisitBlock(parenthesizedLambdaExpression.Block); }

            additionalFunctions.Add(new StaticMethodFunction
            {
                MethodOf = "Lambdas",
                Name = $"Lambda{lambdaFunctionIndex}",
                Parameters = parenthesizedLambdaExpression.ParameterList.Parameters.Select(p => new MethodParameter
                {
                    Name = p.Identifier.Text,
                    TypeName = (p.Type != null) ? VisitType(p.Type, true, false) : "void*"
                }).ToList(),
                ReturnTypeName = VisitType(returnType),
                Statements = new List<Statement> { lambdaBlock }
            });

            var functionPointerReference = new PrefixUnaryExpression
            {
                OperatorSymbol = "&",
                Operand = new Literal
                {
                    LiteralText = $"Lambdas_Lambda{lambdaFunctionIndex}"
                }
            };

            lambdaFunctionIndex += 1;
            return functionPointerReference;
        }

        private Expression VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax interpolatedStringExpression)
        {
            var interpolations = new List<CixFunctionInvocation>();
            var interpolationIndex = 0;
            var stringBuilder = new StringBuilder();

            foreach (var syntaxElement in interpolatedStringExpression.Contents)
            {
                if (syntaxElement is InterpolatedStringTextSyntax interpolatedText)
                {
                    stringBuilder.Append(interpolatedText.TextToken.Text);
                }
                else if (syntaxElement is InterpolationSyntax interpolation)
                {
                    var interpolatedExpression = VisitExpression(interpolation.Expression);
                    var format = interpolation.FormatClause?.FormatStringToken.Text ?? "";
                    interpolations.Add(new CixFunctionInvocation
                    {
                        FunctionName = "String_Format",
                        Arguments = new List<Expression>
                        {
                            interpolatedExpression,
                            new Literal
                            {
                                LiteralText = $"\"{format}\"",
                                TypeName = "byte*"
                            }
                        }
                    });

                    stringBuilder.Append($"{{{interpolationIndex}}}");
                    interpolationIndex += 1;
                }
                else { throw new ArgumentException(syntaxElement.Kind().ToString()); }
            }

            return new CixFunctionInvocation
            {
                FunctionName = "String_Format_Array",
                Arguments = new List<Expression>
                {
                    new Literal
                    {
                        LiteralText = $"\"{stringBuilder.ToString()}\"",
                        TypeName = "byte*"
                    },
                    new CixFunctionInvocation
                    {
                        FunctionName = $"MakeArray{interpolations.Count}",
                        Arguments = interpolations.Cast<Expression>().ToList()
                    }
                }
            };
        }

        private Expression VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccessExpression)
        {
            var expressionType = model.GetTypeInfo(conditionalAccessExpression.Expression).Type;
            var conditionallyAccessedExpression = VisitExpression(conditionalAccessExpression.Expression);
            var cast = new CixFunctionInvocation
            {
                FunctionName =
                    $"Cast{VisitType(expressionType)}ToVoidPointer",
                Arguments = new List<Expression> { conditionallyAccessedExpression }
            };

            var leftNullEvaluation = new BinaryExpression
            {
                Left = cast,
                Right = new Literal
                {
                    LiteralText = "0",
                    TypeName = "void*"
                },
                OperatorSymbol = "!="
            };

            return new CixFunctionInvocation
            {
                FunctionName = $"If_{expressionType}",
                Arguments = new List<Expression>
                {
                    leftNullEvaluation,
                    new BinaryExpression
                    {
                        Left = conditionallyAccessedExpression,
                        Right = VisitExpression(conditionalAccessExpression.WhenNotNull),
                        OperatorSymbol = "."
                    },
                    new Literal
                    {
                        LiteralText = "0",
                        TypeName = "void*"
                    }
                }
            };
        }

        private Expression VisitAssignmentExpression(AssignmentExpressionSyntax assignment) =>
            new BinaryExpression
            {
                Left = VisitExpression(assignment.Left),
                Right = VisitExpression(assignment.Right),
                OperatorSymbol = assignment.OperatorToken.Text
            };

        public string GetTypeForNumericLiteral(string numericLiteral)
        {
            if (numericLiteral.Contains(".")) { return "double"; }

            if (numericLiteral.EndsWith("u", StringComparison.InvariantCultureIgnoreCase)) { return "uint"; }
            if (numericLiteral.EndsWith("l", StringComparison.InvariantCultureIgnoreCase)) { return "long"; }
            if (numericLiteral.EndsWith("ul", StringComparison.InvariantCultureIgnoreCase)) { return "ulong"; }
            if (numericLiteral.EndsWith("f", StringComparison.InvariantCultureIgnoreCase)) { return "float"; }
            if (numericLiteral.EndsWith("d", StringComparison.InvariantCultureIgnoreCase)) { return "double"; }
            if (numericLiteral.EndsWith("m", StringComparison.InvariantCultureIgnoreCase)) { return "decimal"; }

            if (numericLiteral.Contains("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                if (numericLiteral.Length <= 10) { return "int"; }
                if (numericLiteral.Length <= 17) { return "long"; }
                return numericLiteral[3] < '8' ? "long" : "ulong";
            }

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

            if (objectCreation.ArgumentList == null)
            {
                var initializer = objectCreation.Initializer;
                var initializationExpressions = initializer.Expressions.Select(VisitExpression);
                return new ClassStaticFunctionInvocation
                {
                    TypeName = typeName,
                    FunctionName = "ctor_init",
                    Arguments = initializationExpressions.ToList()
                };
            }

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
                case SymbolKind.NamedType:
                case SymbolKind.Namespace:
                    return new Literal
                    {
                        LiteralText = ((Literal)leftExpression).LiteralText + "_" + memberSymbol.Name,
                    };
                case SymbolKind.Event:
                    return new BinaryExpression
                    {
                        Left = leftExpression,
                        Right = new Literal
                        {
                            LiteralText = memberSymbol.Name,
                            TypeName = "void*"
                        }
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
