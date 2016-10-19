namespace Compiler.AST
{
    class LoopNode : Node
    {
        public LoopNode(Node next, Node inner) : base(next)
        {
            Inner = inner;
        }

        public Node Inner { get; private set; }

        public override Node WithNext(Node next)
        {
            return new LoopNode(next, Inner);
        }

        public Node WithInner(Node inner)
        {
            return new LoopNode(Next, inner);
        }
    }
}
