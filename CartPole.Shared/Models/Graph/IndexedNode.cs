using DirectedAcyclicGraph.Interfaces;
using DirectedAcyclicGraph.Models;

namespace CartPoleShared.Models.Graph;

public class IndexedNode(
    string label,
    NodeType type,
    HashSet<DirectedNode> parents,
    HashSet<DirectedNode> children,
    double bias,
    int index,
    Guid id
) : WeightedNode(label, type, parents, children, bias, id), INodeCreatable<IndexedNode>
{
    public int Index { get; } = index;

    public IndexedNode(
        string label,
        NodeType type,
        HashSet<DirectedNode> parents,
        HashSet<DirectedNode> children,
        double bias,
        int index
    )
        : this(label, type, parents, children, bias, index, Guid.NewGuid()) { }

    public IndexedNode CloneNode(
        IndexedNode node,
        HashSet<IndexedNode> parents,
        HashSet<IndexedNode> children
    ) => new(node.Label, node.Type, [.. parents], [.. children], node.Bias, node.Index, node.Id);
}
