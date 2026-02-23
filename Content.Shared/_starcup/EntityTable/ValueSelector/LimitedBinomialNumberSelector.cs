using Content.Shared.EntityTable.ValueSelector;
using Robust.Shared.Random;

namespace Content.Shared._starcup.EntityTable.ValueSelector;

public sealed partial class LimitedBinomialNumberSelector : BinomialNumberSelector
{
    /// <summary>
    /// The minimum number to select. Mainly useful when you want to select over a Gaussian distribution but at least 1.
    /// </summary>
    [DataField]
    public int Minimum = 1;

    public override int Get(System.Random rand)
    {
        var amount = base.Get(rand);
        return Math.Max(amount, Minimum);
    }
}
