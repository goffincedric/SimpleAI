namespace CartPoleShared.Constants;

public static class TrainingConstants
{
    public const double AgentsPerGeneration = 100; // 1000 agents per generation

    public const double TopPerformersPercentage = 0.15; // 15% of the top performers are selected
    public const double DoubleMutationPercentage = 0.2; // 20% of the remaining agents get 2 mutations
    public const double QuadrupleMutationPercentage = 0.25; // 25% of the remaining agents get 4 mutations
}
