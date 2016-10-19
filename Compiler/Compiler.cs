using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Compiler.AST;

namespace Compiler
{
    class Compiler : ASTPass
    {
        private string source;
        private IEnumerable<Token> tokens;

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

        public override void DoPass()
        {
            Tokenise();
            Parse();
        }

        private void Tokenise()
        {
            tokens = from c in source
                     where tokenLookup.ContainsKey(c)
                     select tokenLookup[c];
        }

        private void Parse()
        {
            AST = ParseTokens(tokens.GetEnumerator());
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
    }
}
