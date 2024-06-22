using DirectedAcyclicGraph.Models;

namespace DirectedAcyclicGraph.Interfaces;

public interface INodeCreatable<TNode>
    where TNode : DirectedNode
{
    TNode CloneNode(TNode node, HashSet<TNode> parents, HashSet<TNode> children);
}
