namespace CartPoleShared.Helpers;

public static class AngleHelpers
{
    public static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

    public static double RadiansToDegrees(double radians) => radians * (180 / Math.PI);

    /// <summary>
    /// Returns a percentage between 0 and 1 based on the angle. Down is 0, up is 1. Horizontal is 0.5.
    /// </summary>
    public static double AngleUpPercentage(double radians)
    {
        var degrees = Math.Abs(RadiansToDegrees(radians) % 360);

        // Mirror the angle if it is more than 180 degrees
        if (degrees >= 180)
            degrees = Math.Abs(degrees % 180 - 180);

        // Calculate the percentage based on the angle
        return 1 - degrees / 180;
    }
}
