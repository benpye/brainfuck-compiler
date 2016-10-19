using System;
using System.Collections.Generic;
using System.Linq;

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
                case Token.DecData: return new DataNode(ParseTokens(ts), -1);
                case Token.IncData: return new DataNode(ParseTokens(ts), 1);
                case Token.DecPtr:  return new PtrNode(ParseTokens(ts), -1);
                case Token.IncPtr:  return new PtrNode(ParseTokens(ts), 1);
                case Token.In:      return new InputNode(ParseTokens(ts));
                case Token.Out:     return new OutputNode(ParseTokens(ts));
                case Token.LoopStart:
                    var inner = ParseTokens(ts);
                    return new LoopNode(ParseTokens(ts), inner);
                case Token.LoopEnd: return null;
            }

            throw new Exception($"Unexpected token {cur}");
        }
    }
}
