using Content.Client.Overlays;
using Content.Shared._Impstation.SalvoHud;
using Content.Shared.Materials;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Impstation.SalvoHud;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShowMaterialCompositionIconsSystem : EquipmentHudSystem<ShowMaterialCompositionIconsComponent>
{

    //todo fixedPrice showing as well
    //todo salvager rtb / sell / scrap marker?

    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicalCompositionComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(Entity<PhysicalCompositionComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive)
            return;

        foreach (var (id, _) in entity.Comp.MaterialComposition)
        {
            if (_protoMan.TryIndex<MaterialCompositionIconPrototype>(id, out var proto))
                args.StatusIcons.Add(proto);
        }
    }
}
