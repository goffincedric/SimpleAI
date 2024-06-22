namespace CartPoleShared.Functions;

public static class FitnessFunctions
{
    /// <summary>
    /// Calculate the fitness of the AI based on the angle of the pole. The higher the pole, the higher the score.
    /// If the top of the pole is below the threshold, the score is 0.
    /// </summary>
    /// <param name="poleLength"></param>
    /// <param name="poleAngleRadians"></param>
    /// <param name="heightThresholdPercentage"></param>
    /// <returns></returns>
    public static double PoleTopAboveThreshold(
        double poleLength,
        double poleAngleRadians,
        double maxPoleHeightReward,
        double heightThresholdPercentage = 0.8
    )
    {
        // Calculate the height of the pole
        var poleHeight = Math.Sin(poleAngleRadians) * poleLength;
        var percentage = 1 / poleLength * poleHeight;
        // If the pole height percentage is below threshold, return 0 score
        if (percentage < heightThresholdPercentage)
            return 0;
        // If the pole height percentage is above threshold, return a score scaled from 0 to MaxPoleHeightReward
        // based on how far above the threshold the top of the pole is.
        return (percentage - heightThresholdPercentage)
            / (maxPoleHeightReward - heightThresholdPercentage);
    }

    // Changes to perform: Function that gives more points the closer the cart is to the center of the track
    // Changes to perform: Function that gives more points the less brisk the cart movement is
    // Changes to perform: Function that gives less points, the more distance was traveled
}
