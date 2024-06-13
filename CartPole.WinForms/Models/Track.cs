using SkiaSharp;

namespace CartPoleWinForms.Models;

public class Track(double length, SKColor color)
{
    public double Length { get; } = length;
    public SKColor Color { get; } = color;
}
