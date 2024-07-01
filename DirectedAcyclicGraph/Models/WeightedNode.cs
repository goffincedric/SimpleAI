using System.Diagnostics.CodeAnalysis;

namespace Graphs.Models;

public record WeightedNode
{
    public required Guid Id { get; init; }
    public required string Label { get; init; }
    public required NodeType Type { get; init; }

    public required Dictionary<WeightedNode, double> Parents { get; init; }
    public required HashSet<WeightedNode> Children { get; init; }

    public required Func<double, double, double> ActivationFunction { get; init; }

    public required double Bias { get; set; }

    [SetsRequiredMembers]
    public WeightedNode(
        string label,
        NodeType type,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children,
        double bias,
        Func<double, double, double> activationFunction
    )
        : this(Guid.NewGuid(), label, type, parents, children, bias, activationFunction) { }

    [SetsRequiredMembers]
    public WeightedNode(
        Guid id,
        string label,
        NodeType type,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children,
        double bias,
        Func<double, double, double> activationFunction
    )
    {
        if (type == NodeType.Input && parents.Count > 0)
            throw new ArgumentException("Start nodes cannot have parents.");
        Id = id;
        Label = label;
        Type = type;
        Parents = parents;
        Children = children;

        Bias = bias;
        ActivationFunction = activationFunction;
    }

    public void SetBias(double bias) => Bias = bias;

    public double GetEdgeWeight(WeightedNode parent) => Parents.GetValueOrDefault(parent, 0);

    public void SetEdgeWeight(WeightedNode parent, double weight) => Parents[parent] = weight;

    public void RemoveEdgeWeight(WeightedNode parent) => Parents.Remove(parent);

    public double CalculateOutputValue(double inputValue) => ActivationFunction(inputValue, Bias);

    public WeightedNode CloneNode(
        WeightedNode node,
        Dictionary<WeightedNode, double> parents,
        HashSet<WeightedNode> children
    ) =>
        node with
        {
            Parents = new Dictionary<WeightedNode, double>(parents),
            Children = [.. children]
        };
}
