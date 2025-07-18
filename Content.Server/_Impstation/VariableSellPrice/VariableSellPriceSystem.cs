using Content.Server.Atmos.EntitySystems;
using Content.Server.Cargo.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.VariableSellPrice;

/// <summary>
/// This handles...
/// </summary>
public sealed class VariableSellPriceSystem : EntitySystem
{

    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = null!;
    [Dependency] private readonly SharedCargoSystem _cargoSystem = null!;
    [Dependency] private readonly PricingSystem _pricing = null!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EntitySoldEvent>(OnEntitySold);
    }

    private void OnEntitySold(ref EntitySoldEvent ev)
    {
        if (!TryComp<StationBankAccountComponent>(ev.Station, out var stationBankAccount))
            return;

        var sellDict = new Dictionary<string, double>();

        foreach (var entity in ev.Sold)
        {
            var price = _pricing.GetPrice(entity);
            var protoid = MetaData(entity).EntityPrototype?.ID;

            if (TryComp<GasTankComponent>(entity, out var gasTankComponent))
            {
                var gasPrice = _atmosphereSystem.GetPrice(gasTankComponent.Air);
                price -= gasPrice; //to split the value of the container from the value of the gas

                foreach (var gas in gasTankComponent.Air)
                {
                    var gasProto = _prototypeManager.Index<GasPrototype>(gas.gas.ToString());

                    sellDict[gasProto.Name] = gasProto.PricePerMole * gas.moles;
                }
            }

            if (protoid != null)
            {
                sellDict[protoid] = price;
            }
        }

        var spesosDict = _cargoSystem.GetGeneratedspesos(ev.Station);
        if (spesosDict == null)
            return;

        foreach (var key in sellDict.Keys)
        {

            if (spesosDict.TryGetValue(key, out var spesos))
            {
                spesosDict[key] = spesos +  sellDict[key];
            }
            else
            {
                spesosDict.Add(key, sellDict[key]);
            }
        }
    }
}
