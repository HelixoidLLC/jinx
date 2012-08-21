using System.IO;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using jinx.types.Utilities;

namespace jinx
{
    class JavaScriptWalker : SyntaxWalker
    {
        TextWriter _publicCode = new StringWriter();
        TextWriter _writer;

        public JavaScriptWalker(TextWriter writer)
        {
            _writer = writer;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            string className = node.Identifier.GetText();

            _writer.WriteLine("var {0} = function() {{", className);
            _writer.WriteLine("  var self = this;");

            base.VisitClassDeclaration(node);

            _writer.WriteLine("  return {");
            _writer.Write(_publicCode.ToString());
            _writer.WriteLine("  };");
            _writer.WriteLine("}};");
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var fieldInfo = node.Parent as FieldDeclarationSyntax;
            var modifiers = fieldInfo.Modifiers;

            bool bPublic = modifiers.Any(c => c.Kind == SyntaxKind.PublicKeyword);

            string initValue = DefaultValueEmitter.FromType(node.Type.PlainName);

            foreach (var variable in node.Variables)
            {
                var nodeName = variable.Identifier.GetText().ToLowerCamelCase();

                if (variable.Initializer != null)
                    initValue = EmitExpression(variable.Initializer.Value);
                    //base.VisitVariableDeclaration(node);

                if (bPublic)
                    _publicCode.WriteLine("    {0}: {1},", nodeName, initValue);
                else
                    _writer.WriteLine("  var {0} = {1};", nodeName, initValue);
            }

            //base.VisitVariableDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var modifiers = node.Modifiers;

            bool bPublic = modifiers.Any(c => c.Kind == SyntaxKind.PublicKeyword);
/*
            bool bGet = false;
            bool bSet = false;

            foreach (var acc in node.AccessorList.Accessors)
            {
                if (acc.Keyword.Kind == SyntaxKind.GetKeyword)
                {
                    bGet = true;
                }
                if (acc.Keyword.Kind == SyntaxKind.SetKeyword)
                {
                    bSet = true;
                }
            }
*/
            string nodeName = node.Identifier.GetText().ToLowerCamelCase();
            string initValue = DefaultValueEmitter.FromType(node.Type.PlainName);

            if (bPublic)
                _publicCode.WriteLine("    {0}: {1},", nodeName, initValue);
            else
                _writer.WriteLine("  var {0} = {1};", nodeName, initValue);

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            bool bPublic = false;

            var modifiers = node.Modifiers;
            if (modifiers.Any(c => c.Kind == SyntaxKind.PublicKeyword))
                bPublic = true;

            var name = node.Identifier.GetText().ToLowerCamelCase();
            var @params = VisitParameterList(node.ParameterList);

            _writer.WriteLine("  function {0}({1}) {{", name, @params);
            if (bPublic)
                _publicCode.WriteLine("    {0}: {1},", name, name);

            base.VisitMethodDeclaration(node);

            _writer.WriteLine("  }");
            _writer.WriteLine();
        }

        string VisitParameterList(ParameterListSyntax node)
        {
            var @params = new StringBuilder();

            foreach (var param in node.Parameters)
            {
                if (@params.Length != 0)
                    @params.Append(", ");

                @params.Append(param.Identifier.GetText());
            }

            return @params.ToString();
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            _writer.Write("    return ");

            base.VisitReturnStatement(node);

            _writer.WriteLine(";");
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            base.VisitExpressionStatement(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            _writer.Write(EmitExpression(node));
        }

        private string EmitExpression(ExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.AddExpression:
                    return EmitBinaryExpression(node as BinaryExpressionSyntax);

                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                    return EmitLiteralExpression(node as LiteralExpressionSyntax);

                case SyntaxKind.IdentifierName:
                    return EmitIdentifierName(node as IdentifierNameSyntax);
            }

            return "";
        }

        private string EmitExpression(EqualsValueClauseSyntax node)
        {
            var result = new StringBuilder();

            result.Append("= ");
            result.Append(EmitExpression(node.Value));

            return result.ToString();
        }

        private string EmitBinaryExpression(BinaryExpressionSyntax node)
        {
            switch (node.OperatorToken.GetText())
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    break;

                default:
                    System.Diagnostics.Trace.WriteLine("Unsupported operator: " + node.OperatorToken.GetText());
                    break;
            }

            var expr = new StringBuilder();
            expr.Append(EmitExpression(node.Left));
            expr.Append(' ');
            expr.Append(node.OperatorToken.GetText());
            expr.Append(' ');
            expr.Append(EmitExpression(node.Right));

            return expr.ToString();
        }

        public override void VisitBaseExpression(BaseExpressionSyntax node)
        {
            base.VisitBaseExpression(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            _writer.Write(EmitLiteralExpression(node));

            base.VisitLiteralExpression(node);
        }


        private string EmitLiteralExpression(LiteralExpressionSyntax node)
        {
            /*
            switch (node.Kind)
            {
                case SyntaxKind.NumericLiteralExpression:
                    _writer.Write(node.GetText());
                    break;

                case SyntaxKind.StringLiteralExpression:
                    _writer.Write("\"{0}\"", node.GetText());
                    break;
            }
            */

            return node.GetText();
        }

        private string EmitIdentifierName(IdentifierNameSyntax node)
        {
            return node.Identifier.GetText().ToLowerCamelCase();
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            var variable = node.Identifier.GetText();
            var coll = EmitExpression(node.Expression);
            _writer.WriteLine("    for(var {0} in {1}) {{", variable, coll);

            base.VisitForEachStatement(node);

            _writer.WriteLine("    }");
        }
    }
}
