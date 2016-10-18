using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Compiler
    {
        private string source;
        private IEnumerable<Token> tokens;
        private Command tree;

        private static readonly Dictionary<char, Token> tokenLookup
            = new Dictionary<char, Token>()
        {
            { '>', Token.IncPtr },
            { '<', Token.DecPtr },
            { '+', Token.IncData },
            { '-', Token.DecData },
            { '.', Token.Out },
            { ',', Token.In },
            { '[', Token.LoopStart },
            { ']', Token.LoopEnd }
        };

        public Compiler(string source)
        {
            this.source = source;

            tokens = new List<Token>();
        }

        public void Tokenise()
        {
            tokens = from c in source
                     where tokenLookup.ContainsKey(c)
                     select tokenLookup[c];
        }

        public void Parse()
        {
            tree = ParseTokens(tokens.GetEnumerator());
        }

        private Command ParseTokens(IEnumerator<Token> ts)
        {
            if (!ts.MoveNext())
                return null;

            var cur = ts.Current;

            Command c = new Command();

            if(cur == Token.LoopStart)
            {
                c.Inner = ParseTokens(ts);
            }
            else if(cur == Token.LoopEnd)
            {
                return null;
            }

            c.Op = cur;
            c.Next = ParseTokens(ts);

            return c;
        }

        public void DumpTree()
        {
            DumpTree(tree, 0);
        }

        private void DumpTree(Command node, int depth)
        {
            if (node == null) return;

            if (node.Op == Token.LoopStart)
            {
                Console.WriteLine("{0}LOOP START", new String(' ', depth * 2));
                DumpTree(node.Inner, depth + 1);
                Console.WriteLine("{0}LOOP END", new String(' ', depth * 2));
            }

            switch(node.Op)
            {
                case Token.DecData: Console.WriteLine("{0}DECREMENT DATA", new String(' ', depth * 2)); break;
                case Token.IncData: Console.WriteLine("{0}INCREMENT DATA", new String(' ', depth * 2)); break;
                case Token.DecPtr:  Console.WriteLine("{0}DECREMENT PTR",  new String(' ', depth * 2)); break;
                case Token.IncPtr:  Console.WriteLine("{0}INCREMENT PTR",  new String(' ', depth * 2)); break;
                case Token.In:      Console.WriteLine("{0}INPUT",          new String(' ', depth * 2)); break;
                case Token.Out:     Console.WriteLine("{0}OUTPUT",         new String(' ', depth * 2)); break;
            }

            DumpTree(node.Next, depth);
        }

        public void Compile()
        {
            LLVMBackend llvm = new LLVMBackend(tree);
            llvm.Generate();
        }
    }
}
