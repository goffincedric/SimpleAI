using CartPolePhysics.SinglePole.DoublePrecision;
using CartPoleShared.Constants;
using CartPoleShared.Models;
using CartPoleShared.Models.Environment;

namespace CartPoleShared.Helpers;

public static class EnvironmentHelpers
{
    public static CartPole CreateNewCartPole(double poleAngleRad) =>
        new(0, 0.5f, 0.08f, 0.02f, 1, poleAngleRad, 14);

    public static CartSinglePolePhysicsRK4 CreatePhysics(CartPole cartPole) =>
        new((double)EpisodeConstant.MsPerTimeStep / 1000, cartPole.GetState());
}
