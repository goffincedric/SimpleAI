namespace CartPoleShared.Models.Environment;

public class Cart(float width, float height, double x)
{
    public float Width { get; } = width;
    public float Height { get; } = height;

    public double X { get; protected internal set; } = x;
}
