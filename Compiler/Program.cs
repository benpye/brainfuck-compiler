using System;
using System.IO;

using Compiler.AST;

namespace Compiler
{
    class Program
    {
        private static void PrintHelp()
        {
            Console.WriteLine("Compiler.exe <flags> filename");
            Console.WriteLine("\t-h\t\tPrints this help");
            Console.WriteLine("\t-v\t\tVerbose output");
            Console.WriteLine("\t-On\t\tSets optimisation level to n (default 0)");
            Console.WriteLine("\t-o filename\tSets output filename");
            Console.WriteLine("\t-b n\t\tSets the data array to n bytes long (default 30000)");
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            string inPath = args[args.Length - 1];
            uint opt = 0;
            uint bufLen = 30000;
            string outPath = Path.GetFileNameWithoutExtension(inPath) + ".bc";
            bool verbose = false;

            for (int i = 0; i < args.Length - 1; i++)
            {
                var arg = args[i];
                if (arg == "-h")
                {
                    PrintHelp();
                    return;
                }
                else if(arg.StartsWith("-O"))
                {
                    if(!UInt32.TryParse(arg.Substring(2), out opt))
                    {
                        Console.WriteLine("Optimisation level is not a number");
                        PrintHelp();
                        return;
                    }
                }
                else if (arg.StartsWith("-b"))
                {
                    if (i == args.Length - 2)
                    {
                        Console.WriteLine("Expected buffer size after -o");
                        PrintHelp();
                        return;
                    }

                    i++;
                    outPath = args[i];

                    if (!UInt32.TryParse(args[i], out bufLen))
                    {
                        Console.WriteLine("Data array length is not a number");
                        PrintHelp();
                        return;
                    }
                }
                else if(arg == "-o")
                {
                    if(i == args.Length - 2)
                    {
                        Console.WriteLine("Expected path after -o");
                        PrintHelp();
                        return;
                    }

                    i++;
                    outPath = args[i];
                }
                else if(arg == "-v")
                {
                    verbose = true;
                }
                else
                {
                    Console.WriteLine($"Unrecognised argument {arg}");
                    PrintHelp();
                    return;
                }
            }
            
            string input = "";
            
            try
            {
                input = File.ReadAllText(inPath);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error opening input file {e.Message}");
                return;
            }

            Node AST = null;

            if(verbose)
                Console.WriteLine("Parsing: ");

            Compiler c = new Compiler(input);
            c.DoPass();

            if(verbose)
                c.DumpTree();

            AST = c.AST;

            if(verbose)
                Console.WriteLine("Optimising: ");

            if (opt > 0)
            {
                RepeatOptimiser ro = new RepeatOptimiser(AST);
                ro.DoPass();

                if(verbose)
                    ro.DumpTree();

                AST = ro.AST;
            }

            if(verbose)
                Console.WriteLine("Compiling: ");

            LLVMBackend llvm = new LLVMBackend(AST, bufLen);
            llvm.Compile();
            llvm.Optimise(opt);
            try
            {
                llvm.Output(outPath);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Failed to write output {e.Message}");
                return;
            }
        }
    }
}
