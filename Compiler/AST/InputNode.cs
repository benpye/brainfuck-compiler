namespace Compiler.AST
{
    class InputNode : Node
    {
        public InputNode(Node next) : base(next)
        {
        }

        public override Node WithNext(Node next)
        {
            return new InputNode(next);
        }
    }
}
