using SkiaSharp;

namespace CartPoleWinForms.Models;

/// <summary>
///
/// </summary>
/// <param name="x">Initial x-axis position on the track in meters. Center is 0 meters.</param>
/// <param name="cartWidth">Width in meters</param>
/// <param name="cartHeight">Height in meters</param>
/// <param name="cartColor">Color of the cart</param>
/// <param name="poleWidth">Width in meters</param>
/// <param name="poleHeight">Height in meters</param>
/// <param name="poleAngleRad">Pole angle in radians</param>
/// <param name="poleColor">Color of the pole</param>
/// <param name="jointColor">Color of the cart-pole joint</param>
/// <param name="trackLength">Length of the track in meters. Center is 0.</param>
/// <param name="trackColor">Color of the track.</param>
public class CartPole(
    double x,
    float cartWidth,
    float cartHeight,
    SKColor cartColor,
    float poleWidth,
    float poleHeight,
    double poleAngleRad,
    SKColor poleColor,
    SKColor jointColor,
    float trackLength,
    SKColor trackColor
)
{
    public double X => x;

    public Cart Cart { get; } = new(cartWidth, cartHeight, cartColor);
    public Pole Pole { get; } = new(poleWidth, poleHeight, poleAngleRad, poleColor);
    public Joint Joint { get; } = new(poleWidth, jointColor);
    public Track Track { get; } = new(trackLength, trackColor);

    // public SKRect GetCartBoundingBox() =>
    //     new(
    //         // Left
    //         (float)(XPos - (double)Cart.Width / 2),
    //         // Top
    //         (float)(YPos - (double)Cart.Height / 2),
    //         // Right
    //         (float)(XPos + (double)Cart.Width / 2),
    //         // Bottom
    //         (float)(YPos + (double)Cart.Height / 2)
    //     );

    // public SKRect GetPoleBoundingBox() =>
    //     new(
    //         // Left
    //         (float)(XPos - (double)Pole.Width / 2),
    //         // Top
    //         (float)(YPos - Pole.Height),
    //         // Right
    //         (float)(XPos + (double)Pole.Width / 2),
    //         // Bottom
    //         (float)YPos
    //     );

    // public SKPoint GetJoint() => new((float)XPos, (float)YPos);

    public double[] GetState() => [X, 0, Pole.AngleRad, 0];

    public void SetState(double[] state) => (x, Pole.AngleRad) = (state[0], state[2]);
}
