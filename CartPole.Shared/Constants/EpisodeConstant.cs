namespace CartPoleShared.Constants;

public static class EpisodeConstant
{
    public const int TimeStepsPerEpisode = 1000; // 1000 timesteps at 10ms per timestep is 10 seconds
    public const int MsPerTimeStep = 10; // 100Hz of the physics simulation
}
