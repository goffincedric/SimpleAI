using CartPoleShared.Functions;
using DirectedAcyclicGraph.Interfaces;
using DirectedAcyclicGraph.Models;

namespace CartPoleShared.Models.Graph;

public class WeightedNode(
    string label,
    NodeType type,
    HashSet<DirectedNode> parents,
    HashSet<DirectedNode> children,
    double bias,
    Guid id
) : DirectedNode(id, label, type, parents, children), INodeCreatable<WeightedNode>
{
    protected Func<double, double, double> ActivationFunction { get; } =
        type switch
        {
            NodeType.Start => ActivationFunctions.LinearActivation,
            NodeType.Hidden => ActivationFunctions.ReLU,
            NodeType.End => ActivationFunctions.HyperbolicTangent,
            _ => throw new Exception("Unknown node type")
        };

    public double Bias { get; private set; } = bias;
    internal Dictionary<DirectedNode, double> IncomingNodeWeights { get; } = new();

    public WeightedNode(
        string label,
        NodeType type,
        HashSet<DirectedNode> parents,
        HashSet<DirectedNode> children,
        double bias
    )
        : this(label, type, parents, children, bias, Guid.NewGuid()) { }

    public void SetBias(double bias) => Bias = bias;

    public double GetEdgeWeight(DirectedNode parent) =>
        IncomingNodeWeights.GetValueOrDefault(parent, 0);

    public void SetEdgeWeight(DirectedNode parent, double weight) =>
        IncomingNodeWeights[parent] = weight;

    public double CalculateOutputValue(double inputValue) => ActivationFunction(inputValue, Bias);

    public WeightedNode CloneNode(
        WeightedNode node,
        HashSet<WeightedNode> parents,
        HashSet<WeightedNode> children
    ) => new(node.Label, node.Type, [.. parents], [.. children], node.Bias, node.Id);
}
