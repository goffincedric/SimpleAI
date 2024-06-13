namespace DirectedAcyclicGraph.Models;

public class Node(string label, NodeType nodeType)
{
    public string Label { get; } = label;
    public NodeType Type { get; } = nodeType;

    public override string ToString()
    {
        return $"{nameof(Node)} {label} {nodeType.ToString()}";
    }
}
