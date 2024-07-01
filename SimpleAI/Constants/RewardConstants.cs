namespace SimpleAI.Constants;

public static class RewardConstants
{
    public const int MaxPoleAngledUpReward = 2; // Max reward for the pole being fully up
    public const double MaxPoleHeightAboveThresholdReward = 0.0005; // Give a bigger reward for the pole being above the threshold
    public const double MaxCenterPositionReward = 0.05; // Reward for being in the center

    public const int MaxTrackLimitExceedPunishment = -1; // Negate al other rewards if the cart is too far from the center
    public const double MaxPoleVelocityPunishment = -1.5; // The higher the pole velocity, the higher the punishment
}
