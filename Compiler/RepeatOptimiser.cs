using Compiler.AST;

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
            int c;
            var next = node?.Next;

            switch (node)
            {
                case DataNode n:
                    c = n.Change;
                    while(next is DataNode dn)
                    {
                        c += dn.Change;
                        next = next.Next;
                    }
                    node = n.WithChange(c);
                    break;
                case PtrNode n:
                    c = n.Change;
                    while (next is PtrNode pn)
                    {
                        c += pn.Change;
                        next = next.Next;
                    }
                    node = n.WithChange(c);
                    break;
                case LoopNode n:
                    node = n.WithInner(Optimise(n.Inner));
                    break;
            }

            node = node?.WithNext(Optimise(next));

            return node;
        }
    }
}
