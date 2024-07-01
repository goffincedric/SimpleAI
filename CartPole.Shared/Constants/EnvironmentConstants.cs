namespace CartPoleShared.Constants;

public static class EnvironmentConstants
{
    public static class Physics
    {
        public const int TimeStepMs = 10; // 100Hz of the physics simulation
    }

    public static class CartPoleDimensions
    {
        public const double CartStartingPosition = 0d;
        public const float TrackLength = 7f;
        public const float PoleHeight = 2f;
        public const float CartWidth = 1f;
    }
}
