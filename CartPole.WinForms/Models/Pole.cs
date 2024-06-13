﻿using SkiaSharp;

namespace CartPoleWinForms.Models;

public class Pole(float width, float height, double angleRad, SKColor color)
{
    public float Width { get; } = width;
    public float Height { get; } = height;

    /// <summary>
    /// Pole angle in radians. Index 2 in the state array of the physics model. Positive is clockwise.
    /// </summary>
    public double AngleRad { get; set; } = angleRad;

    public SKColor Color { get; } = color;
}
