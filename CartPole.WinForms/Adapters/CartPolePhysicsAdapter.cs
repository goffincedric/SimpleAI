using CartPolePhysics.SinglePole.DoublePrecision;

namespace CartPoleWinForms.Adapters;

public class CartPolePhysicsAdapter(CartSinglePolePhysics physics)
{
    private readonly CartSinglePolePhysics _physics = physics;
}
