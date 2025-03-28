using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpToCixCore
{
	internal sealed class TreeWriter
    {
        private const string InitHeader = "void GlobalInit() {\r\n";

        private int nextCatchBlockStructIndex;
        private ClassTransform currentTransform;

        public string WriteTree(TreeBuilder tree)
        {
            var outputBuilder = new StringBuilder();
            var initBuilder = new StringBuilder(InitHeader);

            foreach (var enumDefine in tree.EnumDefines)
            {
                outputBuilder.AppendLine($"#define {enumDefine.Key} {enumDefine.Value}");
            }

            foreach (var transform in tree.ClassTransforms)
            {
                outputBuilder.AppendLine(WriteClassTransform(transform));
                foreach (var initLine in WriteClassTransformForInit(transform))
                {
                    initBuilder.AppendLine(initLine);
                }
            }

            foreach (var lambdaFunction in tree.AdditionalFunctions)
            {
                outputBuilder.AppendLine(WriteFunction(lambdaFunction));
            }

            foreach (var interfaceDefinition in tree.Interfaces)
            {
                outputBuilder.AppendLine(WriteInterface(interfaceDefinition));
            }

            foreach (var anonymousObjectStruct in tree.AnonymousObjectStructs)
            {
                outputBuilder.AppendLine(WriteStruct(anonymousObjectStruct));
            }

            foreach (var global in tree.Globals)
            {
                outputBuilder.AppendLine($"global {global.TypeName} {global.VariableName};");
            }

            foreach (var globalInitStatement in tree.GlobalInitializerStatements)
            {
                initBuilder.AppendLine(WriteStatement(globalInitStatement, 1, new List<string>()));
            }

            initBuilder.AppendLine("}");
            outputBuilder.AppendLine(initBuilder.ToString());

            return outputBuilder.ToString();
        }

        public string WriteInterface(ClassVTableStructDefinition interfaceDefinition)
        {
            var builder = new StringBuilder();

            builder.AppendLine(WriteVTable(interfaceDefinition));

            return builder.ToString();
        }

        public string WriteClassTransform(ClassTransform transform)
        {
            currentTransform = transform;
			var builder = new StringBuilder();

            var className = transform.ClassName;
            var classIDSymbol = $"{className.ToUpperInvariant()}_TYPE_ID";
            builder.AppendLine($"#define {classIDSymbol} 0x{transform.TypeId:X8}");

            builder.AppendLine(WriteStruct(transform.Members));
            builder.AppendLine(WriteVTable(transform.VTable));

            foreach (var captureStruct in transform.CatchBlockCaptures) { builder.AppendLine(WriteStruct(captureStruct)); }
            foreach (var function in transform.Functions) { builder.AppendLine(WriteFunction(function)); }

            foreach (var staticFunction in transform.StaticFunctions)
            {
                builder.AppendLine(WriteStaticFunction(staticFunction));
            }
            foreach (var catchBlock in transform.CatchBlockFunctions) { builder.AppendLine(WriteFunction(catchBlock)); }

            currentTransform = null;
            return builder.ToString();
        }

        public IEnumerable<string> WriteClassTransformForInit(ClassTransform transform)
        {
            var baseTypeName = transform.Inheritance.BaseTypeName.ToUpperInvariant();
            var derivedTypeName = transform.Inheritance.DerivedTypeName.ToUpperInvariant();
            yield return $"\tTypeInheritsFrom({derivedTypeName}_TYPE_ID, {baseTypeName}_TYPE_ID)";

            foreach (var catchBlock in transform.CatchBlockFunctions)
            {
                var exceptionTypeIdSymbol = $"{catchBlock.Parameters[1].TypeName.ToUpperInvariant().TrimEnd('*')}_TYPE_ID";
                yield return $"\tHandlesExceptionType(&{catchBlock.Name}, {exceptionTypeIdSymbol})";
            }

            var vtableName = $"{transform.ClassName}_vtable";
            foreach (var instanceMethod in transform.VTable.Pointers)
            {
                yield return $"\t{vtableName}.{instanceMethod.FunctionName} = &{transform.ClassName}_{instanceMethod.FunctionName};";
            }
        }

        private string WriteStruct(CixStruct memberStruct)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"struct {memberStruct.Name} {{");

            foreach (var member in memberStruct.Members) { builder.AppendLine($"\t{member.TypeName} {member.Name};"); }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private string WriteVTable(ClassVTableStructDefinition vtable)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"struct {vtable.Name} {{");

            foreach (var pointer in vtable.Pointers)
            {
                var types = String.Join(", ", pointer.ParameterTypeNames.Prepend(pointer.ReturnTypeName));
                builder.AppendLine($"\t@funcptr<{types}> {pointer.FunctionName};");
            }

            builder.AppendLine("}");
            builder.AppendLine(
                $"global {vtable.Name} {vtable.Name.Substring(0, vtable.Name.LastIndexOf("_struct", StringComparison.Ordinal))};");

            return builder.ToString();
        }

        private string WriteFunction(MethodFunction function)
        {
            var builder = new StringBuilder();

            var parameters = String.Join(", ",
                function.Parameters.Select(param => $"{param.TypeName} {param.Name}"));

            var fullFunctionName = $"{function.MethodOf}_{function.Name}";

            builder.AppendLine($"{function.ReturnTypeName} {fullFunctionName}({parameters}) {{");
            builder.AppendLine($"\tCallStackPush(\"{fullFunctionName}\");");

            var locals = GetLocalVariablesOfStatements(function.Statements).Select(l => l.VariableName).ToList();

            if (function.Statements != null)
            {
                foreach (var statement in function.Statements)
                {
                    builder.AppendLine(WriteStatement(statement, 1, locals));
                }
            }

            builder.AppendLine("\tCallStackPop();");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private string WriteStaticFunction(StaticMethodFunction function)
        {
            var builder = new StringBuilder();

            var parameters = String.Join(", ",
                function.Parameters.Select(param => $"{param.TypeName} {param.Name}"));

            var fullFunctionName = $"{function.MethodOf}_{function.Name}";

            builder.AppendLine($"{function.ReturnTypeName} {fullFunctionName}({parameters}) {{");
            builder.AppendLine($"\tCallStackPush(\"{fullFunctionName}\");");

            var locals = GetLocalVariablesOfStatements(function.Statements).Select(l => l.VariableName).ToList();

            if (function.Statements != null)
            {
                foreach (var statement in function.Statements)
                {
                    builder.AppendLine(WriteStatement(statement, 1, locals));
                }
            }

            builder.AppendLine("\tCallStackPop();");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private string WriteStatement(Statement statement, int depth, IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            if (statement is VariableDeclaration variableDeclaration)
            {
                blockLocalNames.Add(variableDeclaration.VariableName);
                return WriteVariableDeclaration(variableDeclaration, depth, blockLocalNames);
            }

            if (statement is ReturnValueStatement returnValueStatement)
            {
                return WriteReturnValueStatement(returnValueStatement, depth, blockLocalNames);
            }

            if (statement is ExpressionStatement expressionStatement)
            {
                return WriteExpressionStatement(expressionStatement, depth, blockLocalNames);
            }

            if (statement is IfStatement ifStatement)
            {
                return WriteIfStatement(ifStatement, depth, blockLocalNames);
            }

            if (statement is Block block) { return WriteBlock(block, depth, blockLocalNames); }

            if (statement is TryCatch tryCatch) { return WriteTryCatchStatement(tryCatch, depth, blockLocalNames); }

            if (statement is SwitchBlock switchBlock) { return WriteSwitchBlock(switchBlock, depth, blockLocalNames); }

            if (statement is KeywordStatement keyword) { return $"{tabs}{keyword.Keyword};"; }

            if (statement is WhileStatement whileStatement) { return WriteWhileStatement(whileStatement, depth, blockLocalNames); }

            if (statement is DoWhileStatement doWhileStatement)
            {
                return WriteDoWhileStatement(doWhileStatement, depth, blockLocalNames);
            }

            if (statement is StatementList statementList)
            {
                var builder = new StringBuilder();
                foreach (var listStatement in statementList.Statements)
                {
                    builder.AppendLine(WriteStatement(listStatement, depth, blockLocalNames));
                }

                return builder.ToString();
            }

            throw new ArgumentException(statement.GetType().Name);
        }

        // WYLO: wow, the try/catch rewrite rules are hard. Let's try again.
        // The try and finally bodies are fairly trivial, so let's get to the hard
        // part: the catch blocks. Each catch block is lifted to its own function,
        // but since a C# catch block can reference variables outside of it (as with
        // all blocks), we need to capture those, too. We capture anything accessed
        // that is a local variable or a field of the containing type. Properties
        // are written as method invocations, and statics are already globals.
        //
        // Capturing the variable does the following:
        //  1. Adds it to a captures struct named Catch{n}_Locals, where {n} is
        //     an incrementing integer. The member's type is a pointer to whatever
        //     the variable's type is. The member's name is either the name of the
        //     local for local variables, or this_{fieldName} for instance fields.
        //  2. Adds it to a series of catch block initializer statements that
        //     start with a declaration and initialization of the struct. The
        //     initialization is of the form
        //
        //         locals->memberName = &memberName;
        //
        //     or
        //
        //         locals->this_memberName = &this->memberName;
        //  3. Rewrites accesses in the catch block for this member. Accesses to
        //     local variables transform as:
        //
        //         x = 0 => locals->x = 0
        //
        //      or
        //
        //         this->x = 0 => locals->this_x = 0
        //      A literal becomes a binary expression, and the binary expression
        //      has its left and right expressions changed.
        // The catch block is lifted into its own method named TypeName_Catch{n}
        // with parameters of types Catch{n}_locals and a pointer to the exception,
        // if any.
        //
        // You can remove the block stack and the local declaration finder, as I don't
        // think we'll need those.

        private string WriteTryCatchStatement(TryCatch tryCatch, int depth, IList<string> blockLocalNames)
        {
            blockLocalNames.AddRange(GetLocalVariablesOfStatement(tryCatch.TryStatement).Names());
            var tryBody = WriteStatement(tryCatch.TryStatement, depth, blockLocalNames);
            var finallyBody = "";

            if (tryCatch.FinallyStatement != null)
            {
                blockLocalNames.AddRange(GetLocalVariablesOfStatement(tryCatch.FinallyStatement).Names());
                finallyBody = WriteStatement(tryCatch.FinallyStatement, depth, blockLocalNames);
            }

            var tabs = new string('\t', depth);
            var builder = new StringBuilder();

            foreach (var catchBlock in tryCatch.CatchBlocks)
            {
                var captureStruct = CaptureAccessedVariablesInStatement("", nextCatchBlockStructIndex, catchBlock.Statement);
                currentTransform.CatchBlockCaptures.Add(captureStruct);

                var captureStructName = $"Catch{nextCatchBlockStructIndex}_Locals*";
                var catchBlockFunction = new StaticMethodFunction
                {
                    MethodOf = currentTransform.ClassName,
                    Name = $"Catch{nextCatchBlockStructIndex}",
                    ReturnTypeName = "void",
                    Parameters = new List<MethodParameter>
                    {
                        new MethodParameter
                        {
                            TypeName = captureStructName,
                            Name = "locals"
                        },
                        new MethodParameter
                        {
                            TypeName = $"{catchBlock.CatchesExceptionType}*",
                            Name = catchBlock.ExceptionName
                        }
                    }
                };

                WalkExpressionsInStatement(catchBlock.Statement,
                    exp =>
                    {
                        foreach (var capturedVariable in captureStruct.Members)
                        {
                            RewriteAllAccessesOfVariable(exp, capturedVariable);
						}
                    });
                catchBlockFunction.Statements = new List<Statement>
                {
                    catchBlock.Statement
                };

                currentTransform.CatchBlockFunctions.Add(catchBlockFunction);
                nextCatchBlockStructIndex++;

                builder.AppendLine($"{tabs}{captureStructName}* locals_{captureStruct.CatchBlockNumber};");
                foreach (var member in captureStruct.Members)
                {
                    var assignmentExpression = (member.Name.StartsWith("this_", StringComparison.Ordinal))
                        ? $"(this->{member.Name.Split('_')[1]})"
                        : $"{member.Name}";

                    builder.AppendLine($"{tabs}locals_{captureStruct.CatchBlockNumber}->{member.Name} = &{assignmentExpression};");
                }

                builder.AppendLine();
            }

            builder.AppendLine(tryBody);
            builder.AppendLine(finallyBody);

            return builder.ToString();
        }

        private string WriteIfStatement(IfStatement ifStatement, int depth, IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            var builder = new StringBuilder();
            blockLocalNames.AddRange(GetLocalVariablesOfStatement(ifStatement.TrueStatement).Names());

            builder.AppendLine($"{tabs}if ({WriteExpression(ifStatement.Condition, blockLocalNames, false)}) {{");
            builder.AppendLine(WriteStatement(ifStatement.TrueStatement, depth + 1, blockLocalNames));
            builder.AppendLine($"{tabs}}}");

            if (ifStatement.ElseStatement != null)
            {
                blockLocalNames.AddRange(GetLocalVariablesOfStatement(ifStatement.ElseStatement).Names());
                builder.AppendLine(WriteStatement(ifStatement.ElseStatement, depth, blockLocalNames));
            }

            return builder.ToString();
        }

        private string WriteExpressionStatement(ExpressionStatement expressionStatement, int depth, IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            return $"{tabs}{WriteExpression(expressionStatement.Expression, blockLocalNames)};";
        }

        private string WriteReturnValueStatement(ReturnValueStatement returnValueStatement, int depth,
            IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            return returnValueStatement.Expression != null ? $"{tabs}return {WriteExpression(returnValueStatement.Expression, blockLocalNames)};" : $"{tabs}return;";
        }

        private string WriteVariableDeclaration(VariableDeclaration variableDeclaration, int depth,
            IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            var declaration = $"{tabs}{variableDeclaration.TypeName} {variableDeclaration.VariableName}";

            if (variableDeclaration.Initializer != null)
            {
                declaration += " = " + WriteExpression(variableDeclaration.Initializer, blockLocalNames);
            }

            return declaration + ";";
        }

        private string WriteBlock(Block block, int depth, IList<string> blockLocalNames)
        {
            blockLocalNames.AddRange(GetLocalVariablesOfBlock(block).Names());

            var tabs = new string('\t', depth);
            var builder = new StringBuilder();

            builder.AppendLine($"{tabs}{{");

            foreach (var statement in block.Statements) { builder.AppendLine(WriteStatement(statement, depth + 1, blockLocalNames)); }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private string WriteSwitchBlock(SwitchBlock switchBlock, int depth, IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            var builder = new StringBuilder();
            var expression = WriteExpression(switchBlock.Expression, blockLocalNames);

            builder.AppendLine($"{tabs}switch ({expression}) {{");

            foreach (var section in switchBlock.Cases)
            {
                foreach (var label in section.Labels)
                {
                    builder.AppendLine(label.Value != null
                        ? $"{tabs}\tcase {label.Value.LiteralText}:"
                        : $"{tabs}\tdefault:");
                }

                blockLocalNames.AddRange(GetLocalVariablesOfStatement(section.Body).Names());
                builder.AppendLine(WriteStatement(section.Body, depth + 2, blockLocalNames));
            }

            builder.AppendLine($"{tabs}}}");
            return builder.ToString();
        }

        private string WriteWhileStatement(WhileStatement whileStatement, int depth,
            IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            var builder = new StringBuilder();
            blockLocalNames.AddRange(GetLocalVariablesOfStatement(whileStatement.Statement).Names());

            builder.AppendLine($"{tabs}while ({WriteExpression(whileStatement.Expression, blockLocalNames)}) {{");
            builder.AppendLine(WriteStatement(whileStatement.Statement, depth + 1, blockLocalNames));
            builder.AppendLine($"{tabs}}}");

            return builder.ToString();
        }

        private string WriteDoWhileStatement(DoWhileStatement doWhileStatement,
            int depth,
            IList<string> blockLocalNames)
        {
            var tabs = new string('\t', depth);
            var builder = new StringBuilder();
            blockLocalNames.AddRange(GetLocalVariablesOfStatement(doWhileStatement.Statement).Names());

            builder.AppendLine($"{tabs} do {{");
            builder.AppendLine(WriteStatement(doWhileStatement.Statement, depth + 1, blockLocalNames));
            builder.AppendLine($"}} while ({WriteExpression(doWhileStatement.Expression, blockLocalNames)})");

            return builder.ToString();
        }

        private string WriteExpression(Expression expression, ICollection<string> blockLocalNames, bool ignoreLocals = false)
        {
            if (IsPropertySetterAccess(expression)) { return WriteExpression(RewritePropertySetterAccess((BinaryExpression)expression), blockLocalNames); }

            if (IsIndexerSetterAccess(expression))
            {
                return WriteExpression(RewriteIndexerSetterAccess((BinaryExpression)expression), blockLocalNames);
            }

            if (expression is CixFunctionInvocation cixFunctionInvocation)
            {
                var argumentExpressions = cixFunctionInvocation.Arguments ?? new List<Expression>();
                var arguments = String.Join(", ", argumentExpressions.Select(arg => WriteExpression(arg, blockLocalNames)));
                return $"{cixFunctionInvocation.FunctionName}({arguments})";
            }

            if (expression is Literal literal)
            {
                return (blockLocalNames.Contains(literal.LiteralText) || ignoreLocals || !FirstCharacterIsValidForIdentifier(literal.LiteralText))
                    ? literal.LiteralText
                    : $"this->{literal.LiteralText}";
            }

            if (expression is BinaryExpression binaryExpression)
            {
                return binaryExpression.OperatorSymbol == "->"
                    ? $"{WriteExpression(binaryExpression.Left, blockLocalNames, true)} {binaryExpression.OperatorSymbol} {WriteExpression(binaryExpression.Right, blockLocalNames, true)}"
                    : $"{WriteExpression(binaryExpression.Left, blockLocalNames)} {binaryExpression.OperatorSymbol} {WriteExpression(binaryExpression.Right, blockLocalNames)}";
            }

            if (expression is PrefixUnaryExpression unaryExpression)
            {
                return $"{unaryExpression.OperatorSymbol}{WriteExpression(unaryExpression.Operand, blockLocalNames)}";
            }

            if (expression is ClassStaticFunctionInvocation classStaticFunctionInvocation)
            {
                var argumentExpressions = classStaticFunctionInvocation.Arguments ?? new List<Expression>();
                var arguments = String.Join(", ", argumentExpressions.Select(arg => WriteExpression(arg, blockLocalNames)));
                return $"{classStaticFunctionInvocation.TypeName}_{classStaticFunctionInvocation.FunctionName}({arguments})";
            }

            if (expression is PropertyAccess propertyAccess)
            {
                return WriteExpression(RewritePropertyGetterAccess(propertyAccess), blockLocalNames);
            }
            
            if (expression is ParenthesizedExpression parenthesizedExpression)
            {
                return $"({WriteExpression(parenthesizedExpression.Expression, blockLocalNames)})";
            }

            if (expression is IndexerAccess indexerAccess)
            {
                return WriteExpression(RewriteIndexerGetterAccess(indexerAccess), blockLocalNames);
            }

            if (expression is ClassVirtualFunctionInvocation virtualFunctionInvocation)
            {
                var argumentExpressions = virtualFunctionInvocation.Arguments ?? new List<Expression>();
                var arguments = string.Join(", ", argumentExpressions.Select(arg => WriteExpression(arg, blockLocalNames)));
                return $"{virtualFunctionInvocation.TypeName}_vtable.{virtualFunctionInvocation.FunctionName}({arguments})";
            }

            if (expression is PostfixUnaryExpression postfixUnaryExpression)
            {
                return
                    $"{WriteExpression(postfixUnaryExpression.Operand, blockLocalNames)}{postfixUnaryExpression.OperatorSymbol}";
            }

            throw new ArgumentException(expression.GetType().Name);
        }

        private static bool IsPropertySetterAccess(Expression expression)
        {
            if (!(expression is BinaryExpression binaryExpression)) { return false; }

            return binaryExpression.Left is PropertyAccess && binaryExpression.OperatorSymbol == "=";
        }

        private static ClassStaticFunctionInvocation RewritePropertyGetterAccess(PropertyAccess access) =>
            new ClassStaticFunctionInvocation
            {
                TypeName = access.TypeName,
                FunctionName = access.PropertyName + "_get",
                Arguments = new List<Expression> { access.LeftExpression }
            };

        private static ClassStaticFunctionInvocation RewritePropertySetterAccess(BinaryExpression expression)
        {
            var propertyAccess = (PropertyAccess)expression.Left;
            var value = expression.Right;

            return new ClassStaticFunctionInvocation
            {
                TypeName = propertyAccess.TypeName,
                FunctionName = propertyAccess.PropertyName + "_set",
                Arguments = new List<Expression> { propertyAccess.LeftExpression, value }
            };
        }

        private static bool IsIndexerSetterAccess(Expression expression)
        {
            if (!(expression is BinaryExpression binaryExpression)) { return false; }

            return binaryExpression.Left is IndexerAccess && binaryExpression.OperatorSymbol == "=";
        }

        private static ClassStaticFunctionInvocation RewriteIndexerGetterAccess(IndexerAccess access) =>
            new ClassStaticFunctionInvocation
            {
                TypeName = access.TypeName,
                FunctionName = "get",
                Arguments = access.Arguments.Prepend(access.Expression).ToList()
            };

        private static ClassStaticFunctionInvocation RewriteIndexerSetterAccess(BinaryExpression expression)
        {
            var indexerAccess = (IndexerAccess)expression.Left;
            var value = expression.Right;

            return new ClassStaticFunctionInvocation
            {
                TypeName = indexerAccess.TypeName,
                FunctionName = "set",
                Arguments = indexerAccess.Arguments.Prepend(indexerAccess.Expression).Append(value).ToList()
            };
        }

        private static CatchBlockCapturesStruct CaptureAccessedVariablesInStatement(string functionName,
            int catchBlockIndex,
            Statement statement)
        {
            var capturedVariables = new List<StructMember>();

			capturedVariables.AddRange(GetCapturableMemberAccessesInStatement(statement)
				.Select(MemberAccessToCapturedVariable));

            return new CatchBlockCapturesStruct
            {
                ForFunction = functionName,
                CatchBlockNumber = catchBlockIndex,
                Members = capturedVariables
            };
        }

        private static StructMember MemberAccessToCapturedVariable(Expression v)
        {
            switch (v)
            {
                case Literal memberAccess:
                    return new StructMember
                    {
                        TypeName = memberAccess.TypeName + "*",
                        Name = memberAccess.LiteralText
                    };

                case BinaryExpression thisMemberAccess:
                    var rightLiteral = (Literal)thisMemberAccess.Right;
                    return new StructMember
                    {
                        Name = $"this_{rightLiteral.LiteralText}",
                        TypeName = rightLiteral.TypeName + "*"
                    };

                default: throw new InvalidOperationException();
            }
        }

        private static Expression RewriteMemberAccessInCatchBlock(Expression expression)
        {
            switch (expression)
            {
                case Literal memberAccess:
                    return new BinaryExpression
                    {
                        Left = new Literal { LiteralText = "locals" },
                        Right = memberAccess,
                        OperatorSymbol = "->"
                    };
                case BinaryExpression thisMemberAccess:
                    return new BinaryExpression
                    {
                        Left = new Literal { LiteralText = "locals" },
                        Right = new Literal { LiteralText = $"this_{((Literal)thisMemberAccess.Right).LiteralText}" }
                    };
                default: throw new InvalidOperationException();
            }
        }

        private static bool IsAccessedOfCapturedVariable(Expression expression, StructMember variable)
        {
            if (!IsCapturableMemberAccess(expression)) { return false; }

            if (expression is Literal memberAccess) { return memberAccess.LiteralText == variable.Name; }

            if (expression is BinaryExpression binaryExpression && variable.Name.StartsWith("this_", StringComparison.Ordinal))
            {
                var rightLiteral = (Literal)binaryExpression.Right;
                return rightLiteral.LiteralText == variable.Name.Split('_')[1];
            }

            return false;
        }

        private static void RewriteAllAccessesOfVariable(Expression expression, StructMember variable)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                if (IsAccessedOfCapturedVariable(binaryExpression.Left, variable))
                {
                    binaryExpression.Left = RewriteMemberAccessInCatchBlock(binaryExpression.Left);
                }
                else
                {
                    RewriteAllAccessesOfVariable(binaryExpression.Left, variable);
                }

                if (IsAccessedOfCapturedVariable(binaryExpression.Right, variable))
                {
                    binaryExpression.Right = RewriteMemberAccessInCatchBlock(binaryExpression.Right);
                }
                else
                {
                    RewriteAllAccessesOfVariable(binaryExpression.Right, variable);
                }
            }

            if (expression is PrefixUnaryExpression unaryExpression)
            {
                if (IsAccessedOfCapturedVariable(unaryExpression.Operand, variable))
                {
                    unaryExpression.Operand = RewriteMemberAccessInCatchBlock(unaryExpression.Operand);
                }
                else
                {
                    RewriteAllAccessesOfVariable(unaryExpression.Operand, variable);
                }
            }

            if (expression is ClassStaticFunctionInvocation classStaticFunctionInvocation)
            {
				for (var i = 0; i < classStaticFunctionInvocation.Arguments.Count; i++)
                {
					var argument = classStaticFunctionInvocation.Arguments[i];
					if (IsAccessedOfCapturedVariable(argument, variable))
                    {
                        classStaticFunctionInvocation.Arguments[i] = RewriteMemberAccessInCatchBlock(argument);
                    }
                    else
                    {
                        RewriteAllAccessesOfVariable(argument, variable);
                    }
                }
            }

            if (expression is CixFunctionInvocation cixFunctionInvocation)
            {
                for (var i = 0; i < cixFunctionInvocation.Arguments.Count; i++)
                {
                    var argument = cixFunctionInvocation.Arguments[i];
                    if (IsAccessedOfCapturedVariable(argument, variable))
                    {
                        cixFunctionInvocation.Arguments[i] = RewriteMemberAccessInCatchBlock(argument);
                    }
                    else { RewriteAllAccessesOfVariable(argument, variable); }
                }
            }

            if (expression is PropertyAccess propertyAccess)
            {
                if (IsAccessedOfCapturedVariable(propertyAccess.LeftExpression, variable))
                {
                    propertyAccess.LeftExpression = RewriteMemberAccessInCatchBlock(propertyAccess.LeftExpression);
                }
                else
                {
                    RewriteAllAccessesOfVariable(propertyAccess.LeftExpression, variable);
                }
            }

            if (expression is IndexerAccess indexerAccess)
            {
                if (IsAccessedOfCapturedVariable(indexerAccess.Expression, variable))
                {
                    indexerAccess.Expression = RewriteMemberAccessInCatchBlock(indexerAccess.Expression);
                }
                else { RewriteAllAccessesOfVariable(indexerAccess.Expression, variable); }

                for (var i = 0; i < indexerAccess.Arguments.Count; i++)
                {
                    var argument = indexerAccess.Arguments[i];
                    if (IsAccessedOfCapturedVariable(argument, variable))
                    {
                        indexerAccess.Arguments[i] = RewriteMemberAccessInCatchBlock(argument);
                    }
                    else { RewriteAllAccessesOfVariable(argument, variable); }
                }
            }
        }

        private static IEnumerable<VariableDeclaration> GetLocalVariablesOfBlock(Block block)
        {
            foreach (var statement in block.Statements)
            {
                if (statement is VariableDeclaration local) { yield return local; }
            }
        }

        private static IEnumerable<VariableDeclaration> GetLocalVariablesOfStatement(Statement statement)
        {
            if (statement is Block block)
            {
                foreach (var local in GetLocalVariablesOfBlock(block))
                {
                    yield return local;
                }
            }

            if (statement is VariableDeclaration declaration) { yield return declaration; }
        }

        private static IEnumerable<VariableDeclaration> GetLocalVariablesOfStatements(IEnumerable<Statement> statements)
        {
            foreach (var statement in statements)
            {
                if (statement is VariableDeclaration local) { yield return local; }
            }
        }

        private static IEnumerable<Expression> GetCapturableMemberAccessesInBlock(Block block)
        {
            foreach (var statement in block.Statements)
            {
                foreach (var access in GetCapturableMemberAccessesInStatement(statement))
                {
                    yield return access;
                }
            }
        }

		private static bool FirstCharacterIsValidForIdentifier(string s) =>
            "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Any(c => s[0] == c);

		private static bool IsCapturableMemberAccess(Expression expression)
        {
            if (expression is Literal literal) { return FirstCharacterIsValidForIdentifier(literal.LiteralText); }

            if (!(expression is BinaryExpression binaryExpression)) { return false; }

            if (binaryExpression.OperatorSymbol != "->") { return false; }

            if (!(binaryExpression.Left is Literal leftLiteral)) { return false; }

            if (binaryExpression.Right is Literal rightLiteral)
            {
                return leftLiteral.LiteralText == "this" &&
                    FirstCharacterIsValidForIdentifier(rightLiteral.LiteralText);
            }

            return false;
        }

        private static IEnumerable<Expression> GetCapturableMemberAccessesInStatement(Statement statement)
        {
            while (true)
            {
                switch (statement)
                {
                    case ExpressionStatement expressionStatement:
                    {
                        foreach (var access in WhereRecursive(expressionStatement.Expression, IsCapturableMemberAccess)) { yield return access; }

                        break;
                    }
                    case VariableDeclaration variableDeclaration when variableDeclaration.Initializer != null:
                    {
                        foreach (var access in WhereRecursive(variableDeclaration.Initializer, IsCapturableMemberAccess)) { yield return access; }

                        break;
                    }
                    case ReturnValueStatement returnValue when returnValue.Expression != null:
                    {
                        foreach (var access in WhereRecursive(returnValue.Expression, IsCapturableMemberAccess)) { yield return access; }

                        break;
                    }
                    case IfStatement ifStatement:
                    {
                        foreach (var access in WhereRecursive(ifStatement.Condition, IsCapturableMemberAccess)) { yield return access; }

                        foreach (var access in GetCapturableMemberAccessesInStatement(ifStatement.TrueStatement)) { yield return access; }

                        foreach (var access in GetCapturableMemberAccessesInStatement(ifStatement.ElseStatement)) { yield return access; }

                        break;
                    }
                    case Block childBlock:
                    {
                        foreach (var access in GetCapturableMemberAccessesInBlock(childBlock)) { yield return access; }

                        break;
                    }
                    case TryCatch tryCatch:
                    {
                        foreach (var access in GetCapturableMemberAccessesInStatement(tryCatch.TryStatement)) { yield return access; }

                        foreach (var catchBlock in tryCatch.CatchBlocks)
                        {
                            foreach (var access in GetCapturableMemberAccessesInStatement(catchBlock.Statement)) { yield return access; }
                        }

                        foreach (var access in GetCapturableMemberAccessesInStatement(tryCatch.FinallyStatement)) { yield return access; }

                        break;
                    }
                    case SwitchBlock switchBlock:
                    {
                        foreach (var access in WhereRecursive(switchBlock.Expression, IsCapturableMemberAccess)) { yield return access; }

                        foreach (var switchSection in switchBlock.Cases)
                        {
                            foreach (var access in GetCapturableMemberAccessesInStatement(switchSection.Body)) { yield return access; }
                        }

                        break;
                    }
                    case WhileStatement whileStatement:
                    {
                        foreach (var access in WhereRecursive(whileStatement.Expression, IsCapturableMemberAccess)) { yield return access; }

                        statement = whileStatement.Statement;
                        continue;
                    }
                }

                break;
            }
        }

        private static void WalkExpressionsInStatement(Statement statement, Action<Expression> action)
        {
            while (true)
            {
                switch (statement)
                {
                    case ExpressionStatement expressionStatement:
                        action(expressionStatement.Expression);
                        break;

                    case VariableDeclaration variableDeclaration when variableDeclaration.Initializer != null:
                        action(variableDeclaration.Initializer);
                        break;

                    case ReturnValueStatement returnValue when returnValue.Expression != null:
                        action(returnValue.Expression);
                        break;

                    case IfStatement ifStatement:
                    {
                        action(ifStatement.Condition);
                        WalkExpressionsInStatement(ifStatement.TrueStatement, action);
                        if (ifStatement.ElseStatement != null)
                        {
                            statement = ifStatement.ElseStatement;
                            continue;
                        }

                        break;
                    }

                    case Block childBlock:
                    {
                        foreach (var childStatement in childBlock.Statements) { WalkExpressionsInStatement(childStatement, action); }

                        break;
                    }

                    case TryCatch tryCatch:
                    {
                        WalkExpressionsInStatement(tryCatch.TryStatement, action);
                        if (tryCatch.FinallyStatement != null) { WalkExpressionsInStatement(tryCatch.FinallyStatement, action); }

                        foreach (var catchBlock in tryCatch.CatchBlocks) { WalkExpressionsInStatement(catchBlock.Statement, action); }

                        break;
                    }

                    case SwitchBlock switchBlock:
                    {
                        action(switchBlock.Expression);
                        foreach (var switchSection in switchBlock.Cases) { WalkExpressionsInStatement(switchSection.Body, action); }

                        break;
                    }

                    case WhileStatement whileStatement:
                        action(whileStatement.Expression);
                        statement = whileStatement.Statement;
                        continue;
                }

                break;
            }
        }

        private static IEnumerable<Expression> WhereRecursive(Expression expression, Func<Expression, bool> predicate)
        {
            if (predicate(expression)) { yield return expression; }

            if (expression is BinaryExpression binaryExpression)
            {
                foreach (var match in WhereRecursive(binaryExpression.Left, predicate)) { yield return match; }
                foreach (var match in WhereRecursive(binaryExpression.Right, predicate)) { yield return match; }
            }

            if (expression is PrefixUnaryExpression unaryExpression)
            {
                foreach (var match in WhereRecursive(unaryExpression.Operand, predicate)) { yield return match; }
            }

            if (expression is ClassStaticFunctionInvocation classStaticFunctionInvocation)
            {
                foreach (var argument in classStaticFunctionInvocation.Arguments)
                {
                    foreach (var match in WhereRecursive(argument, predicate)) { yield return match; }
                }
            }

            if (expression is CixFunctionInvocation cixFunctionInvocation)
            {
                foreach (var argument in cixFunctionInvocation.Arguments)
                {
                    foreach (var match in WhereRecursive(argument, predicate)) { yield return match; }
                }
            }

            if (expression is PropertyAccess propertyAccess)
            {
                foreach (var match in WhereRecursive(propertyAccess.LeftExpression, predicate)) { yield return match; }
            }

            if (expression is IndexerAccess indexerAccess)
            {
                foreach (var match in WhereRecursive(indexerAccess.Expression, predicate)) { yield return match; }

                foreach (var argument in indexerAccess.Arguments)
                {
                    foreach (var match in WhereRecursive(argument, predicate)) { yield return match; }
                }
            }
        }
    }
}
