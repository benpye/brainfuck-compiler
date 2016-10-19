namespace Compiler.AST
{
    public abstract class Node
    {
        public Node(Node next)
        {
            Next = next;
        }

        public Node Next { get; private set; }

        public abstract Node WithNext(Node next);
    }
}
