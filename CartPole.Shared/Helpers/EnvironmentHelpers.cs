using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Functions;
using CartPoleShared.Models.Environment;
using Graphs.Functions;
using Graphs.Models;

namespace CartPoleShared.Helpers;

public static class EnvironmentHelpers
{
    public static DirectedAcyclicGraph GenerateStartingGraph()
    {
        // Create empty graph
        var graph = new DirectedAcyclicGraph();

        // Input nodes:
        // 0 = x-axis coordinate of the cart (metres).
        // 1 = x-axis velocity of the cart (m/s).
        // 2 = Pole angle (radians); deviation from the vertical. Positive is clockwise.
        // 3 = Pole angular velocity (radians/s). Positive is clockwise.
        var inputNode1 = GraphFunctions.CreateInputNode(
            "CartPole position",
            0,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var inputNode2 = GraphFunctions.CreateInputNode(
            "Cart velocity",
            1,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var inputNode3 = GraphFunctions.CreateInputNode(
            "Pole angle",
            2,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );
        var inputNode4 = GraphFunctions.CreateInputNode(
            "Pole angular velocity",
            3,
            ActivationFunctions.ResolveActivationFunction(NodeType.Input)
        );

        // Output node:
        // Force applied to the cart (in Newtons).
        var outputNode = GraphFunctions.CreateOutputNode(
            "Force applied to cart",
            0,
            ActivationFunctions.ResolveActivationFunction(NodeType.Output)
        );

        // Add nodes to the graph
        graph.AddNode(inputNode1, []);
        graph.AddNode(inputNode2, []);
        graph.AddNode(inputNode3, []);
        graph.AddNode(inputNode4, []);
        graph.AddNode(outputNode, []);

        // Connect all input nodes to the output node
        graph.AddEdge(inputNode1, outputNode);
        graph.AddEdge(inputNode2, outputNode);
        graph.AddEdge(inputNode3, outputNode);
        graph.AddEdge(inputNode4, outputNode);

        // Return graph
        return graph;
    }

    public static CartPole CreateNewCartPole(double cartPosition, double poleAngleRad) =>
        new(
            cartPosition,
            EnvironmentConstants.CartPoleDimensions.CartWidth,
            EnvironmentConstants.CartPoleDimensions.CartWidth / 12.5f,
            EnvironmentConstants.CartPoleDimensions.PoleHeight / 50f,
            EnvironmentConstants.CartPoleDimensions.PoleHeight,
            poleAngleRad,
            EnvironmentConstants.CartPoleDimensions.TrackLength * 2
        );

    public static CartSinglePolePhysicsRK4 CreatePhysics(CartPole cartPole) =>
        new(
            (double)EnvironmentConstants.Physics.TimeStepMs / 1000,
            cartPole.GetState(),
            new CartSinglePoleEquations(3, 0.1, 1.0, 1, 0.001, 0.1)
        );
}
