using CartPoleShared.Helpers;

namespace CartPoleShared.Functions;

public static class FitnessFunctions
{
    public static double CalculateFitness(Func<double>[] fitnessFunction) =>
        fitnessFunction.Sum(f => f());

    /// <summary>
    /// Calculate the fitness of the AI based on the angle of the pole. The higher the pole, the higher the score.
    /// If the top of the pole is below the threshold, the score is 0.
    /// </summary>
    /// <param name="poleAngleRadians"></param>
    /// <param name="maxPoleHeightReward"></param>
    /// <param name="heightThresholdPercentage"></param>
    /// <returns></returns>
    public static double PoleTopAboveThreshold(
        double poleAngleRadians,
        double maxPoleHeightReward,
        double heightThresholdPercentage = 0.9
    )
    {
        // Calculate how angled up the pole is in percentage
        var poleUpPercentage = AngleHelpers.AngleUpPercentage(poleAngleRadians);
        // If the pole height percentage is below threshold, return 0 score
        if (poleUpPercentage < heightThresholdPercentage)
            return 0;
        // If the pole height percentage is above threshold, return a score scaled from 0 to MaxPoleHeightReward
        // based on how far above the threshold the top of the pole is.
        return (poleUpPercentage - heightThresholdPercentage)
            / (maxPoleHeightReward - heightThresholdPercentage);
    }

    /// <summary>
    /// Calculate the fitness of the AI based on the angle of the pole. The higher the pole, the higher the score.
    /// If the top of the pole is below the threshold, the score is 0.
    /// </summary>
    /// <param name="poleAngleRadians"></param>
    /// <param name="maxPoleHeightReward"></param>
    /// <param name="heightThresholdPercentage"></param>
    /// <returns></returns>
    public static double PoleTopNotMovingAboveThreshold(
        double poleAngleRadians,
        double poleVelocity,
        double maxPoleHeightReward,
        double heightThresholdPercentage = 0.8
    )
    {
        // Calculate how angled up the pole is in percentage
        var poleUpPercentage = AngleHelpers.AngleUpPercentage(poleAngleRadians);
        // If the pole height percentage is below threshold, return 0 score
        if (poleUpPercentage < heightThresholdPercentage)
            return 0;
        // If the pole height percentage is above threshold, return the reward divided by the pole velocity
        var reward = maxPoleHeightReward / Math.Abs(Math.Max(0.01, poleVelocity));
        return reward;
    }

    /// <summary>
    /// Returns a reward between 0 and 1 based on the pole angle. Down is 0, up is 1.
    /// </summary>
    public static double PoleHeight(double poleAngleRadians, double maxReward) =>
        AngleHelpers.AngleUpPercentage(poleAngleRadians) * maxReward;

    /// <summary>
    /// Returns a negative reward for exceeding a specified distance from the center of the track.
    /// </summary>
    public static double PunishBeingOffTrack(
        double cartPosition,
        double distanceFromCenter,
        double maxPunishment
    )
    {
        var normalizedCartPosition = Math.Abs(cartPosition);
        // If the cart is within the distance from the center, don't punish
        if (normalizedCartPosition <= distanceFromCenter)
            return 0;
        return -Math.Abs(maxPunishment);
    }

    public static double RewardBeingInCenter(double cartPosition, double maxReward)
    {
        var normalizedCartPosition = Math.Abs(cartPosition);
        // If the cart is within the distance from the center, reward
        if (normalizedCartPosition <= 0.01)
            return maxReward;
        return 0;
    }

    public static double PunishHighPoleVelocity(double poleVelocity, double maxPunishment)
    {
        const double maxPoleVelocity = 5;
        var normalizedPoleVelocity = Math.Abs(poleVelocity);
        // Punish more the closer to max velocity
        if (normalizedPoleVelocity >= maxPoleVelocity)
            return -10;
        return -Math.Abs(maxPunishment * (normalizedPoleVelocity / maxPoleVelocity));
    }

    // Changes to perform: Function that gives more points the closer the cart is to the center of the track
    // Changes to perform: Function that gives more points the less brisk the cart movement is
    // Changes to perform: Function that gives less points, the more distance was traveled
}
