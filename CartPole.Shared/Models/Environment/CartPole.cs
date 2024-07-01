namespace CartPoleShared.Models.Environment;

public class CartPole
{
    public Cart Cart { get; }
    public Pole Pole { get; }
    public Track Track { get; }

    /// <param name="x">Initial x-axis position on the track in meters. Center is 0 meters.</param>
    /// <param name="cartWidth">Width in meters</param>
    /// <param name="cartHeight">Height in meters</param>
    /// <param name="poleWidth">Width in meters</param>
    /// <param name="poleHeight">Height in meters</param>
    /// <param name="poleAngleRad">Pole angle in radians</param>
    /// <param name="trackLength">Length of the track in meters. Center is 0.</param>
    public CartPole(
        double x,
        float cartWidth,
        float cartHeight,
        float poleWidth,
        float poleHeight,
        double poleAngleRad,
        float trackLength
    )
    {
        Cart = new Cart(cartWidth, cartHeight, x);
        Pole = new Pole(poleWidth, poleHeight, poleAngleRad, 0);
        Track = new Track(trackLength);
    }

    public double[] GetState() => [Cart.X, 0, Pole.AngleRadians, Pole.Velocity];

    public void SetState(double[] state) =>
        (Cart.X, Pole.AngleRadians, Pole.Velocity) = (state[0], state[2], state[3]);
}
