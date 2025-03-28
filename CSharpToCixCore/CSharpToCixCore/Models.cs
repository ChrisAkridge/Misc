using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpToCixCore
{
    public class ClassTransform
    {
        public string ClassName { get; set; }
        public InheritanceRegistration Inheritance { get; set; }
        public ClassMemberStruct Members { get; set; }
        public ClassVTableStructDefinition VTable { get; set; }
        public int TypeId { get; set; }
        public ClassConstructorFunction Constructor { get; set; }
        public List<InstanceMethodFunction> Functions { get; set; }
        public List<StaticMethodFunction> StaticFunctions { get; set; }
        public List<CatchBlockCapturesStruct> CatchBlockCaptures { get; set; }
        public List<StaticMethodFunction> CatchBlockFunctions { get; set; }
    }

    public class CixStruct
    {
        public virtual string Name { get; set; }
        public List<StructMember> Members { get; set; }
    }

    public class ClassMemberStruct : CixStruct
    {
    }

    public class StructMember
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    public class ClassVTableStructDefinition
    {
        public string Name { get; set; }
        public List<ClassVTableFunctionPointer> Pointers { get; set; }
    }

    public class ClassVTableFunctionPointer
    {
        public string ReturnTypeName { get; set; }
        public List<string> ParameterTypeNames { get; set; }
        public string FunctionName { get; set; }
    }

    public class ClassConstructorFunction
    {
        public string ConstructorFor { get; set; }
        public List<MethodParameter> Parameters { get; set; }
        public List<Statement> Statements { get; set; }
    }

    public abstract class MethodFunction
    {
        public string Name { get; set; }
        public string ReturnTypeName { get; set; }
        public string MethodOf { get; set; }
        public List<MethodParameter> Parameters { get; set; }
        public List<Statement> Statements { get; set; }
    }

    public class InstanceMethodFunction : MethodFunction { }

    public class StaticMethodFunction : MethodFunction { }

    public class MethodParameter
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    public class InheritanceRegistration
    {
        public string BaseTypeName { get; set; }
        public string DerivedTypeName { get; set; }
    }

    public class CatchBlockCapturesStruct : CixStruct
    {
        public string ForFunction { get; set; }
        public int CatchBlockNumber { get; set; }

        public override string Name
        {
            get => $"Catch{CatchBlockNumber}";
            set => throw new InvalidOperationException();
        }
    }

    public abstract class Statement { }

    public abstract class Expression { }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public string OperatorSymbol { get; set; }
    }

    public class PrefixUnaryExpression : Expression
    {
        public string OperatorSymbol { get; set; }
        public Expression Operand { get; set; }
    }

    public class PostfixUnaryExpression : Expression
    {
        public Expression Operand { get; set; }
        public string OperatorSymbol { get; set; }
    }

    public class VariableDeclaration : Statement
    {
        public string TypeName { get; set; }
        public string VariableName { get; set; }
        public Expression Initializer { get; set; }
    }

    public class ClassStaticFunctionInvocation : Expression
    {
        public string TypeName { get; set; }
        public string FunctionName { get; set; }
        public List<Expression> Arguments { get; set; }
    }

    public class ClassVirtualFunctionInvocation : Expression
    {
        public string TypeName { get; set; }
        public string FunctionName { get; set; }
        public List<Expression> Arguments { get; set; }
        public Expression ThisArgument { get; set; }
    }

    public class CixFunctionInvocation : Expression
    {
        public string FunctionName { get; set; }
        public List<Expression> Arguments { get; set; }
    }

    public class Literal : Expression
    {
        public string LiteralText { get; set; }
        public string TypeName { get; set; }
    }

    public class PropertyAccess : Expression
    {
        public Expression LeftExpression { get; set; }
        public string PropertyName { get; set; }
        public bool IsStatic { get; set; }
        public string TypeName { get; set; }
    }

    public class ParenthesizedExpression : Expression
    {
        public Expression Expression { get; set; }
    }

    public class ReturnValueStatement : Statement
    {
        public Expression Expression { get; set; }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        public Statement TrueStatement { get; set; }
        public Statement ElseStatement { get; set; }
    }

    public class Block : Statement
    {
        public List<Statement> Statements { get; set; }
    }

    public class StatementList : Statement
    {
        public List<Statement> Statements { get; set; }
    }

    public class IndexerAccess : Expression
    {
        public Expression Expression { get; set; }
        public List<Expression> Arguments { get; set; }
        public string TypeName { get; set; }
    }

    public class TryCatch : Statement
    {
        public Statement TryStatement { get; set; }
        public List<CatchBlock> CatchBlocks { get; set; }
        public Statement FinallyStatement { get; set; }
    }

    public class CatchBlock : Statement
    {
        public string CatchesExceptionType { get; set; }
        public string ExceptionName { get; set; }
        public Statement Statement { get; set; }
    }

    public class SwitchBlock : Statement
    {
        public Expression Expression { get; set; }
        public List<SwitchSection> Cases { get; set; }
    }

    public class SwitchSection
    {
        public List<SwitchLabel> Labels { get; set; }
        public Statement Body { get; set; }
    }

    public class SwitchLabel
    {
        public Literal Value { get; set; }
    }

    public class KeywordStatement : Statement
    {
        public string Keyword { get; set; }
    }

    public class WhileStatement : Statement
    {
        public Expression Expression { get; set; }
        public Statement Statement { get; set; }
    }

    public class DoWhileStatement : Statement
    {
        public Statement Statement { get; set; }
        public Expression Expression { get; set; }
    }
}
