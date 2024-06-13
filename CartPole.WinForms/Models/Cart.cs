using SkiaSharp;

namespace CartPoleWinForms.Models;

public class Cart(float width, float height, SKColor color)
{
    public float Width { get; } = width;
    public float Height { get; } = height;
    public SKColor Color { get; } = color;
}
