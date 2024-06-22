namespace CartPoleShared.Models.Environment;

/// <summary>
///
/// </summary>
/// <param name="x">Initial x-axis position on the track in meters. Center is 0 meters.</param>
/// <param name="cartWidth">Width in meters</param>
/// <param name="cartHeight">Height in meters</param>
/// <param name="poleWidth">Width in meters</param>
/// <param name="poleHeight">Height in meters</param>
/// <param name="poleAngleRad">Pole angle in radians</param>
/// <param name="trackLength">Length of the track in meters. Center is 0.</param>
public class CartPole(
    double x,
    float cartWidth,
    float cartHeight,
    float poleWidth,
    float poleHeight,
    double poleAngleRad,
    float trackLength
)
{
    public double X => x;

    public Cart Cart { get; } = new(cartWidth, cartHeight);
    public Pole Pole { get; } = new(poleWidth, poleHeight, poleAngleRad);
    public Track Track { get; } = new(trackLength);

    public double[] GetState() => [X, 0, Pole.AngleRadians, 0];

    public void SetState(double[] state) => (x, Pole.AngleRadians) = (state[0], state[2]);
}
