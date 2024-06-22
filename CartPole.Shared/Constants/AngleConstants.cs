using CartPoleShared.Helpers;

namespace CartPoleShared.Constants;

public static class AngleConstants
{
    public static class Degrees
    {
        public const double Up = 0;
        public const double Right = 90;
        public const double Down = 180;
        public const double Left = 270;
    }

    public static class Radians
    {
        public static readonly double Up = AngleHelpers.DegreesToRadians(Degrees.Up);
        public static readonly double Right = AngleHelpers.DegreesToRadians(Degrees.Right);
        public static readonly double Down = AngleHelpers.DegreesToRadians(Degrees.Down);
        public static readonly double Left = AngleHelpers.DegreesToRadians(Degrees.Left);
    }
}
