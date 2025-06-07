using System.Linq;
using System.Numerics;
using Content.Client.Overlays;
using Content.Shared._Impstation.SalvoHud;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Materials;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Impstation.SalvoHud;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShowMaterialCompositionIconsSystem : EquipmentHudSystem<ShowMaterialCompositionIconsComponent>
{

    //todo fixedPrice showing as well
    //todo salvager rtb / sell / scrap marker?

    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;

    private SalvoHudScanOverlay _overlay = default!;

    //yaay storing state in a system
    //makes things easier + means I'm not re-getting the salvohud ent for every status icon
    ShowMaterialCompositionIconsComponent? iconsComp = null;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicalCompositionComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);

        if (_overlayMan.TryGetOverlay<SalvoHudScanOverlay>(out var overlay))
        {
            _overlay = overlay;
        }
        else
        {
            _overlay = new SalvoHudScanOverlay();
            _overlayMan.AddOverlay(_overlay);
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        //play the hits
        if (!_timing.IsFirstTimePredicted)
            return;

        //update salvohuds
        //don't really care if they're out of sync w/ the server because their stuff only ever gets shown clientside?
        //can fix later if it's an issue
        var query = EntityQueryEnumerator<ShowMaterialCompositionIconsComponent>();
        while (query.MoveNext(out var comp))
        {
            switch (comp.CurrState)
            {
                case SalvohudScanState.Idle:
                    //constantly reset to a given state if idle
                    comp.CurrMaxRange = 0;
                    comp.CurrMinRange = 0;
                    comp.Accumulator = 0;
                    comp.LastPingPos = null;
                    break;

                case SalvohudScanState.In:
                    comp.Accumulator += frameTime;
                    var inProgress = comp.Accumulator / comp.InPeriod;
                    comp.CurrMaxRange = comp.MaxRange * (inProgress * inProgress);

                    _overlay.ScanPoint ??= comp.LastPingPos;

                    if (!(comp.Accumulator > comp.InPeriod))
                        break;

                    comp.Accumulator = 0;
                    comp.CurrState = SalvohudScanState.Active;
                    break;

                case SalvohudScanState.Active:
                    comp.Accumulator += frameTime;
                    comp.CurrMaxRange = comp.MaxRange;

                    if (!(comp.Accumulator > comp.ActivePeriod))
                        break;

                    comp.Accumulator = 0;
                    comp.CurrState = SalvohudScanState.Out;
                    break;

                case SalvohudScanState.Out:
                    comp.Accumulator += frameTime;
                    var outProgress = comp.Accumulator / comp.OutPeriod;
                    comp.CurrMinRange = comp.MaxRange * (outProgress * outProgress);

                    if (!(comp.Accumulator > comp.OutPeriod))
                        break;

                    comp.CurrState = SalvohudScanState.Idle;
                    break;
            }
        }

        //vaguely evil thing to get the salvohud the player is currently wearing
        iconsComp = null;
        if (_playerMan.LocalEntity is not {} player || !TryComp<InventoryComponent>(player, out var inventoryComp))
            return;

        var invEnumerator = new InventorySystem.InventorySlotEnumerator(inventoryComp, SlotFlags.EYES);
        while (invEnumerator.MoveNext(out var inventorySlot))
        {
            if (inventorySlot.ContainedEntity != null && HasComp<ShowMaterialCompositionIconsComponent>(inventorySlot.ContainedEntity))
                iconsComp = Comp<ShowMaterialCompositionIconsComponent>(inventorySlot.ContainedEntity.Value);
        }

        if (iconsComp == null)
        {
            _overlay.MinRadius = 0;
            _overlay.MaxRadius = 0;
            _overlay.ScanPoint = null;
        }
        else
        {
            _overlay.MinRadius = iconsComp.CurrMinRange;
            _overlay.MaxRadius = iconsComp.CurrMaxRange;
            _overlay.ScanPoint = iconsComp.LastPingPos;
        }
    }

    private void OnGetStatusIconsEvent(Entity<PhysicalCompositionComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive || iconsComp == null || iconsComp.CurrState == SalvohudScanState.Idle || iconsComp.LastPingPos == null)
            return;

        var diff = _xform.GetWorldPosition(entity) - iconsComp.LastPingPos;
        var dist = diff.Value.Length();

        if (dist > iconsComp.CurrMaxRange || dist < iconsComp.CurrMinRange)
            return;

        foreach (var (id, _) in entity.Comp.MaterialComposition.OrderByDescending(x => x.Value))
        {
            if (_protoMan.TryIndex<MaterialCompositionIconPrototype>(id, out var proto))
                args.StatusIcons.Add(proto);
        }
    }
}
