using System;
using System.IO;
using jinx;

namespace jinxc
{
    class Program
    {
        static void Main(string[] args)
        {
            TextWriter output = Console.Out;

            if (args.Length > 2)
            {
                try
                {
                    output = new StreamWriter(args[1]);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to create output file.");
                    return;
                }
            }

            new JavaScriptCompiler().CompileFile(output, args[0]);

            output.Flush();
            output.Close();
        }
    }
}
