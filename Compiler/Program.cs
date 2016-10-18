using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Program
    {
        private static void Main(string[] args)
        {
            Compiler c = new Compiler(@"+[[-]+]>.>>>>.<<<<-.>>-.>.<<.>>>>-.<<<<<++.>>++.");
            c.Tokenise();
            c.Parse();
            c.DumpTree();
            c.Compile();
        }
    }
}
