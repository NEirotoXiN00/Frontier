using Content.Shared.Chat;
using Content.Shared.Radio;
using Content.Shared.StationBounties;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._NF.BountyContracts;

/// <summary>
///     Store all bounty contracts information.
/// </summary>
[RegisterComponent]
[Access(typeof(BountyContractsSystem))]
public sealed class BountyContractsDataComponent : Component
{
    /// <summary>
    ///     Last registered contract id. Used to track contracts.
    /// </summary>
    [DataField("lastId")]
    public uint LastId;

    /// <summary>
    ///     All open bounty contracts by their contract id.
    /// </summary>
    [DataField("contracts")]
    public Dictionary<uint, BountyContract> Contracts = new();
}
