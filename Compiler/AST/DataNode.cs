namespace Compiler.AST
{
    public class DataNode : Node
    {
        public DataNode(Node next, int change) : base(next)
        {
            Change = change;
        }

        public int Change { get; private set; }

        public override string ToString() => $"{base.ToString()}(Change: {Change})";

        public override Node WithNext(Node next)
        {
            return new DataNode(next, Change);
        }

        public Node WithChange(int change)
        {
            return new DataNode(Next, change);
        }
    }
}
