namespace CartPoleShared.Models.Environment;

public class Pole(float width, float height, double angleRadians, double velocity)
{
    public float Width { get; } = width;
    public float Height { get; } = height;

    /// <summary>
    /// Pole angle in radians. Index 2 in the state array of the physics model. Positive is clockwise.
    /// </summary>
    public double AngleRadians { get; set; } = angleRadians;
    public double Velocity { get; set; } = velocity;
}
