using DirectedAcyclicGraph.Interfaces;

namespace DirectedAcyclicGraph.Models;

public class DirectedNode : Node, INodeCreatable<DirectedNode>
{
    public HashSet<DirectedNode> Parents { get; }
    public HashSet<DirectedNode> Children { get; }

    public DirectedNode(Guid id, string label, NodeType type)
        : this(id, label, type, [], []) { }

    public DirectedNode(
        Guid id,
        string label,
        NodeType type,
        HashSet<DirectedNode> parents,
        HashSet<DirectedNode> children
    )
        : base(id, label, type)
    {
        if (type == NodeType.Start && parents.Count > 0)
            throw new ArgumentException("Start nodes cannot have parents.");
        Parents = parents;
        Children = children;
    }

    public DirectedNode CloneNode(
        DirectedNode node,
        HashSet<DirectedNode> parents,
        HashSet<DirectedNode> children
    ) => new(node.Id, node.Label, node.Type, [.. parents], [.. children]);
}
