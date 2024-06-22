namespace CartPoleShared.Functions;

public static class ActivationFunctions
{
    /// <summary>
    /// Activation function used for input nodes
    /// </summary>
    public static double LinearActivation(double inputValue, double bias) => inputValue + bias;

    /// <summary>
    /// Activation function used for output nodes
    /// </summary>
    public static double HyperbolicTangent(double inputValue, double bias) =>
        Math.Tanh(inputValue + bias);

    /// <summary>
    /// Activation function used for hidden nodes
    /// </summary>
    public static double ReLU(double inputValue, double bias) => Math.Max(0, inputValue + bias);
}
