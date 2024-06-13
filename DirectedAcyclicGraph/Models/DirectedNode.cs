namespace DirectedAcyclicGraph.Models;

public class DirectedNode : Node
{
    public HashSet<DirectedNode> Parents { get; init; }
    public HashSet<DirectedNode> Children { get; init; }

    public DirectedNode(
        string label,
        NodeType nodeType,
        HashSet<DirectedNode> parents,
        HashSet<DirectedNode> children
    )
        : base(label, nodeType)
    {
        Parents = parents;
        Children = children;
    }

    public DirectedNode(Node node, HashSet<DirectedNode> parents, HashSet<DirectedNode> children)
        : base(node.Label, node.Type)
    {
        Parents = parents;
        Children = children;
    }
}
