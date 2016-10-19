using System;

namespace Compiler
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Parsing: ");
            Compiler c = new Compiler(@"++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.");
            c.DoPass();
            c.DumpTree();

            Console.WriteLine("Optimising: ");
            RepeatOptimiser ro = new RepeatOptimiser(c.AST);
            ro.DoPass();
            ro.DumpTree();

            Console.WriteLine("Compiling: ");
            LLVMBackend llvm = new LLVMBackend(c.AST);
            llvm.Compile();
            llvm.Optimise(3);
            llvm.DumpModule();
        }
    }
}
