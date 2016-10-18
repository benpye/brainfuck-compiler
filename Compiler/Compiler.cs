using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Compiler.AST;

namespace Compiler
{
    class Compiler
    {
        private string source;
        private IEnumerable<Token> tokens;
        private Node tree;

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

        private Node ParseTokens(IEnumerator<Token> ts)
        {
            if (!ts.MoveNext())
                return null;

            var cur = ts.Current;

            switch(ts.Current)
            {
                case Token.DecData:   return new DataNode()   { Change = -1, Next = ParseTokens(ts) };
                case Token.IncData:   return new DataNode()   { Change = 1,  Next = ParseTokens(ts) };
                case Token.DecPtr:    return new PtrNode()    { Change = -1, Next = ParseTokens(ts) };
                case Token.IncPtr:    return new PtrNode()    { Change = 1,  Next = ParseTokens(ts) };
                case Token.In:        return new InputNode()  { Next = ParseTokens(ts) };
                case Token.Out:       return new OutputNode() { Next = ParseTokens(ts) };
                case Token.LoopStart: return new LoopNode()   { Inner = ParseTokens(ts), Next = ParseTokens(ts) };
                case Token.LoopEnd:   return null;
            }

            throw new Exception($"Unexpected token {cur}");
        }

        public void DumpTree()
        {
            DumpTree(tree, 0);
        }

        private void DumpTree(Node node, int depth)
        {
            if (node == null) return;

            Console.WriteLine($"{new String(' ', depth * 2)}{node.ToString()}");

            if(node is LoopNode n)
                DumpTree(n.Inner, depth + 1);

            DumpTree(node.Next, depth);
        }

        public void Compile()
        {
            LLVMBackend llvm = new LLVMBackend(tree);
            llvm.Generate();
        }
    }
}
