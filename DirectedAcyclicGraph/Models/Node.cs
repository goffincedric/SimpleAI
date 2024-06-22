namespace DirectedAcyclicGraph.Models;

public class Node(Guid id, string label, NodeType type)
{
    public Guid Id { get; } = id;
    public string Label { get; } = label;
    public NodeType Type { get; } = type;

    public override string ToString()
    {
        return $"{Label} {GetType().Name.ToLower()}";
    }
}
