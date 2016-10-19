using Compiler.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class RepeatOptimiser : ASTPass
    {
        public RepeatOptimiser(Node tree)
        {
            AST = tree;
        }

        public override void DoPass()
        {
            AST = Optimise(AST);
        }

        private Node Optimise(Node node)
        {
            if (node == null)
                return null;

            var next = node.Next;
            int c;
            switch (node)
            {
                case DataNode n:
                    c = n.Change;
                    while(next is DataNode dn)
                    {
                        c += dn.Change;
                        next = next.Next;
                    }
                    return new DataNode() { Change = c, Next = Optimise(next) };
                case PtrNode n:
                    c = n.Change;
                    while (next is PtrNode pn)
                    {
                        c += pn.Change;
                        next = next.Next;
                    }
                    return new PtrNode() { Change = c, Next = Optimise(next) };
                case LoopNode n:
                    return new LoopNode() { Inner = Optimise(n.Inner), Next = Optimise(next) };
                case InputNode n:
                    return new InputNode() { Next = Optimise(next) };
                case OutputNode n:
                    return new OutputNode() { Next = Optimise(next) };
            }

            throw new Exception($"Unexpected node of type {node.GetType()}");
        }
    }
}
