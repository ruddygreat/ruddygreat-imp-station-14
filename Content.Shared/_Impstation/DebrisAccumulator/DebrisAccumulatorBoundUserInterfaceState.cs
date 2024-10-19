using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.DebrisAccumulator;

[Serializable, NetSerializable]
public sealed class DebrisAccumulatorBoundUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan? EndTime;
    public TimeSpan NextOffer;

    public TimeSpan Cooldown;
    public TimeSpan Duration;

    public int ActiveSeed;

    public List<int> Offers;

    public DebrisAccumulatorBoundUserInterfaceState(List<int> offers)
    {
        Offers = offers;
    }
}
