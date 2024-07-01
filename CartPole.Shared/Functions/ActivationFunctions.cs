using Graphs.Models;

namespace CartPoleShared.Functions;

public static class ActivationFunctions
{
    /// <summary>
    /// Activation function used for input nodes
    /// </summary>
    private static double LinearActivation(double inputValue, double bias) => inputValue + bias;

    /// <summary>
    /// Activation function used for output nodes
    /// </summary>
    private static double HyperbolicTangent(double inputValue, double bias) =>
        Math.Tanh(inputValue + bias);

    /// <summary>
    /// Activation function used for hidden nodes
    /// </summary>
    private static double ReLU(double inputValue, double bias) => Math.Max(0, inputValue + bias);

    /// <summary>
    /// Activation function used for hidden nodes
    /// </summary>
    private static double LeakyReLU(double inputValue, double bias) =>
        Math.Max(0.1 * (inputValue + bias), inputValue + bias);

    public static Func<double, double, double> ResolveActivationFunction(NodeType nodeType) =>
        nodeType switch
        {
            NodeType.Input => LinearActivation,
            NodeType.Output => LinearActivation,
            NodeType.Hidden => LeakyReLU,
            _ => throw new ArgumentException("Invalid node type")
        };
}
