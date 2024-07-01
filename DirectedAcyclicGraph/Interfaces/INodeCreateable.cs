using Graphs.Models;

namespace Graphs.Interfaces;

public interface INodeCreatable<TNode, TEdgeNode>
    where TNode : WeightedNode
    where TEdgeNode : WeightedNode
{
    TNode CloneNode(TNode node, Dictionary<TEdgeNode, double> parents, HashSet<TEdgeNode> children);
}
