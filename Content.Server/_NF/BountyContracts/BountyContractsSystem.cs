using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.CartridgeLoader;
using Content.Server.GameTicking.Events;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.StationBounties;
using Robust.Shared.Map;

namespace Content.Server._NF.BountyContracts;

/// <summary>
///     Used to control all bounty contracts placed by players.
/// </summary>
public sealed partial class BountyContractsSystem : EntitySystem
{
    private ISawmill _sawmill = default!;

    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("bounty.contracts");

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        InitializeUi();
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        // use nullspace entity to store all information about contracts
        var uid = Spawn(null, MapCoordinates.Nullspace);
        AddComp<BountyContractsDataComponent>(uid);
    }

    private BountyContractsDataComponent? GetContracts()
    {
        // we assume that there is only one bounty database for round
        // if it doesn't exist - game should work fine
        // but players wouldn't able to create/get contracts
        return EntityQuery<BountyContractsDataComponent>().FirstOrDefault();
    }

    /// <summary>
    ///     Try to create a new bounty contract and put it in bounties list.
    /// </summary>
    /// <param name="name">IC name for the contract bounty head. Can be players IC name or custom string.</param>
    /// <param name="reward">Cash reward for completing bounty. Can be zero.</param>
    /// <param name="description">IC description of players crimes, details, etc.</param>
    /// <param name="vessel">IC name of last known bounty vessel. Can be station/ship name or custom string.</param>
    /// <param name="dna">Optional DNA of the bounty head.</param>
    /// <param name="author">Optional bounty poster IC name.</param>
    /// <param name="postToRadio">Should radio message about contract be posted in general radio channel?</param>
    /// <returns>New bounty contract. Null if contract creation failed.</returns>
    public BountyContract? CreateBountyContract(string name, int reward,
        string? description = null, string? vessel = null,
        string? dna = null, string? author = null)
    {
        var data = GetContracts();
        if (data == null)
            return null;

        // create a new contract
        var contractId = data.LastId++;
        var contract = new BountyContract(contractId, name, reward,
            dna, vessel, description, author);

        // try to save it
        if (!data.Contracts.TryAdd(contractId, contract))
        {
            _sawmill.Error($"Failed to create bounty contract with {contractId}! LastId: {data.LastId}.");
            return null;
        }

        return contract;
    }

    /// <summary>
    ///     Try to get a bounty contract by its id.
    /// </summary>
    public bool TryGetContract(uint contractId, [NotNullWhen(true)] out BountyContract? contract)
    {
        contract = null;
        var data = GetContracts();
        if (data == null)
            return false;

        return data.Contracts.TryGetValue(contractId, out contract);
    }

    /// <summary>
    ///     Try to get all bounty contracts available.
    /// </summary>
    public IEnumerable<BountyContract> GetAllContracts()
    {
        var data = GetContracts();
        if (data == null)
            return Enumerable.Empty<BountyContract>();

        return data.Contracts.Values;
    }

    /// <summary>
    ///     Try to remove bounty contract by its id.
    /// </summary>
    /// <returns>True if contract was found and removed.</returns>
    public bool RemoveBountyContract(uint contractId)
    {
        var data = GetContracts();
        if (data == null)
            return false;

        if (!data.Contracts.Remove(contractId))
        {
            _sawmill.Warning($"Failed to remove bounty contract with {contractId}!");
            return false;
        }

        return true;
    }
}
