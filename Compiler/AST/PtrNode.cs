namespace Compiler.AST
{
    class PtrNode : Node
    {
        public PtrNode(Node next, int change) : base(next)
        {
            Change = change;
        }

        public int Change { get; private set; }

        public override string ToString() => $"{base.ToString()}(Change: {Change})";

        public override Node WithNext(Node next)
        {
            return new PtrNode(next, Change);
        }

        public Node WithChange(int change)
        {
            return new PtrNode(Next, change);
        }
    }
}
