using Graphs.Models;

namespace Graphs.Functions;

public static class GraphFunctions
{
    public static WeightedNode UpdateNodeBias(WeightedNode node)
    {
        // Generate random number between -5 and 5
        var biasChange = GenerateRandomNormal(0, 1, null);
        node.SetBias(node.Bias + biasChange);
        return node;
    }

    public static WeightedNode UpdateIncomingNodeWeight(WeightedNode from, WeightedNode to)
    {
        // Generate random number between -1 and 1
        var weightChange = GenerateRandomNormal(0, 0.5, null);
        to.SetEdgeWeight(from, to.GetEdgeWeight(from) + weightChange);
        return from;
    }

    public static IndexedNode CreateInputNode(
        string label,
        int index,
        Func<double, double, double> activationFunction
    ) =>
        new(
            $"Input node: {label}",
            NodeType.Input,
            [],
            [],
            GenerateRandomNormalBias(new Random()),
            activationFunction,
            index
        );

    public static IndexedNode CreateOutputNode(
        string label,
        int index,
        Func<double, double, double> activationFunction
    ) =>
        new(
            $"Output node: {label}",
            NodeType.Output,
            [],
            [],
            GenerateRandomNormalBias(new Random()),
            activationFunction,
            index
        );

    public static WeightedNode CreateHiddenNode(
        string label,
        Func<double, double, double> activationFunction,
        double? bias = null
    ) =>
        new(
            $"Hidden node: {label}",
            NodeType.Hidden,
            [],
            [],
            bias ?? GenerateRandomNormalBias(new Random()),
            activationFunction
        );

    /// <summary>
    /// mean = 0, stdDev = 10
    /// </summary>
    internal static double GenerateRandomNormalBias(Random? random) =>
        GenerateRandomNormal(0, 2, random);

    /// <summary>
    /// mean = 0, stdDev = 5
    /// </summary>
    internal static double GenerateRandomNormalWeight(Random? random = null)
    {
        var weight = GenerateRandomNormal(0, 2, random);
        return weight;
    }

    /// <summary>
    /// Generates a random number from a normal distribution.
    /// </summary>
    private static double GenerateRandomNormal(double mean, double stdDev, Random? random)
    {
        random ??= new Random();
        return mean + stdDev * (random.NextDouble() - 0.5) * 2;
    }
}
