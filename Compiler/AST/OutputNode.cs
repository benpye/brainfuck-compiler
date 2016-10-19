namespace Compiler.AST
{
    class OutputNode : Node
    {
        public OutputNode(Node next) : base(next)
        {
        }

        public override Node WithNext(Node next)
        {
            return new OutputNode(next);
        }
    }
}
