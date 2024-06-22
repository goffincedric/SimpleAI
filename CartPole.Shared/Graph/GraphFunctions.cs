using CartPoleShared.Functions;
using CartPoleShared.Models.Graph;
using DirectedAcyclicGraph.Models;

namespace CartPoleShared.Graph;

public static class GraphFunctions
{
    public static DirectedAcyclicGraph<WeightedNode> GenerateStartingGraph()
    {
        var random = new Random();
        // Create empty graph
        var graph = new DirectedAcyclicGraph<WeightedNode>();

        // Input nodes:
        // 0 = x-axis coordinate of the cart (metres).
        // 1 = x-axis velocity of the cart (m/s).
        // 2 = Pole angle (radians); deviation from the vertical. Positive is clockwise.
        // 3 = Pole angular velocity (radians/s). Positive is clockwise.
        var inputNode1 = new IndexedNode(
            "CartPole position",
            NodeType.Start,
            [],
            [],
            GenerateRandomNormalBias(random),
            0
        );
        var inputNode2 = new IndexedNode(
            "Cart velocity",
            NodeType.Start,
            [],
            [],
            GenerateRandomNormalBias(random),
            1
        );
        var inputNode3 = new IndexedNode(
            "Pole angle",
            NodeType.Start,
            [],
            [],
            GenerateRandomNormalBias(random),
            2
        );
        var inputNode4 = new IndexedNode(
            "Pole angular velocity",
            NodeType.Start,
            [],
            [],
            GenerateRandomNormalBias(random),
            3
        );

        // Output node:
        // Force applied to the cart (in Newtons).
        var outputNode = new IndexedNode(
            "Force applied to cart",
            NodeType.End,
            [],
            [],
            GenerateRandomNormalBias(random),
            0
        );

        // Add nodes to the graph
        graph.AddNode(inputNode1, []);
        graph.AddNode(inputNode2, []);
        graph.AddNode(inputNode3, []);
        graph.AddNode(inputNode4, []);
        graph.AddNode(outputNode, []);

        // Return graph
        return graph;
    }

    public static WeightedNode UpdateNodeBias(WeightedNode node)
    {
        // Generate random number between -0.01 and 0.01
        var biasChange = (double)new Random().Next(20) / 1000 - 0.01;
        node.SetBias(node.Bias + biasChange);
        return node;
    }

    public static WeightedNode UpdateIncomingNodeWeight(
        WeightedNode node,
        WeightedNode incomingNode
    )
    {
        // Generate random number between -0.01 and 0.01
        var weightChange = (double)new Random().Next(20) / 1000 - 0.01;
        node.SetEdgeWeight(incomingNode, node.GetEdgeWeight(incomingNode) + weightChange);
        return node;
    }

    public static WeightedNode CreateHiddenNode(string label) =>
        new(
            "Hidden node: " + label,
            NodeType.Hidden,
            [],
            [],
            GenerateRandomNormalBias(new Random())
        );

    /// <summary>
    /// mean = 0.1, stdDev = 0.1
    /// </summary>
    private static double GenerateRandomNormalBias(Random? random) =>
        GenerateRandomNormal(0, 0.1, random);

    /// <summary>
    /// mean = 0.01, stdDev = 0.1
    /// </summary>
    private static double GenerateRandomNormalWeight(Random? random) =>
        GenerateRandomNormal(0.01, 0.1, random);

    /// <summary>
    /// Generates a random number from a normal distribution.
    /// </summary>
    private static double GenerateRandomNormal(double mean, double stdDev, Random? random)
    {
        random ??= new Random();
        return mean + stdDev * random.NextDouble();
    }
}
