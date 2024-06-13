using System;

namespace CartPoleWinForms.Constants;

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
        public const double Up = Degrees.Up * (Math.PI / 180);
        public const double Right = Degrees.Right * (Math.PI / 180);
        public const double Down = Degrees.Down * (Math.PI / 180);
        public const double Left = Degrees.Left * (Math.PI / 180);
    }
}
