using System.IO;
using Roslyn.Compilers.CSharp;

namespace jinx
{
    public class JavaScriptCompiler
    {
        public void CompileFile(TextWriter output, string path)
        {
            Compile(output, File.ReadAllText(path), path);
        }

        public void Compile(TextWriter output, string content)
        {
            Compile(output, content, "");
        }

        public static string EmitJs(string content)
        {
            var compiler = new JavaScriptCompiler();

            var output = new StringWriter();
            compiler.EmitJs(output, content);

            return output.ToString();
        }

        public void EmitJs(TextWriter output, string content)
        {
            var tree = SyntaxTree.ParseCompilationUnit(content);

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var emitter = new JavaScriptWalker(output);
            emitter.Visit(root);
        }

        private void Compile(TextWriter output, string content, string path)
        {
            var tree = SyntaxTree.ParseCompilationUnit(content, path);

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var collector = new ClassCollector();
            collector.Visit(root);

            foreach (var @class in collector.Classes)
            {
                var emitter = new JavaScriptWalker(output);
                emitter.Visit(@class);
            }
        }
    }
}
