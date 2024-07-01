using System.Diagnostics.CodeAnalysis;
using Graphs.Interfaces;

namespace Graphs.Models;

public record IndexedNode : WeightedNode, INodeCreatable<IndexedNode, WeightedNode>
{
    public required int Index { get; init; }

    [SetsRequiredMembers]
    private IndexedNode(WeightedNode weightedNode, int index)
        : base(weightedNode)
    {
        Index = index;
    }

    [SetsRequiredMembers]
    public IndexedNode(
        string label,
        NodeType type,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children,
        double bias,
        Func<double, double, double> activationFunction,
        int index
    )
        : this(Guid.NewGuid(), label, type, parents, children, bias, activationFunction, index) { }

    [SetsRequiredMembers]
    public IndexedNode(
        Guid id,
        string label,
        NodeType type,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children,
        double bias,
        Func<double, double, double> activationFunction,
        int index
    )
        : base(id, label, type, parents, children, bias, activationFunction)
    {
        Index = index;
    }

    public IndexedNode CloneNode(
        IndexedNode node,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children
    ) => new(base.CloneNode(node, parents, children), node.Index);
}
