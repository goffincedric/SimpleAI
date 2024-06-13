using SkiaSharp;

namespace CartPoleWinForms.Models;

public class Joint(float radius, SKColor color)
{
    public float Radius { get; } = radius;
    public SKColor Color { get; } = color;
}
