using Compiler.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public abstract class ASTPass
    {
        public Node AST { get; protected set; }

        public abstract void DoPass();

        public void DumpTree()
        {
            DumpTree(AST, 0);
        }

        private void DumpTree(Node node, int depth)
        {
            if (node == null) return;

            Console.WriteLine($"{new String(' ', depth * 2)}{node.ToString()}");

            if (node is LoopNode n)
                DumpTree(n.Inner, depth + 1);

            DumpTree(node.Next, depth);
        }
    }
}
