using System.Linq;
using Content.Client.Overlays;
using Content.Shared._Impstation.SalvoHud;
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
    //god I hate all of this code so much I have no idea why it was so painful to write

    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;

    private SalvoHudScanOverlay _overlay = default!;

    //yaay storing state in a system
    //makes things easier + means I'm not re-getting the salvohud ent for every status icon
    private ShowMaterialCompositionIconsComponent? _iconsComp;

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
                    comp.CurrRadius = 0;
                    comp.CurrMinRadius = 0;
                    comp.Accumulator = 0;
                    comp.LastPingPos = null;
                    break;

                case SalvohudScanState.In:
                    comp.Accumulator += frameTime;
                    var inProgress = comp.Accumulator / comp.InPeriod;
                    comp.CurrRadius = comp.MaxRadius * (inProgress * inProgress);

                    _overlay.ScanPoint ??= comp.LastPingPos; //todo don't like this

                    if (!(comp.Accumulator > comp.InPeriod))
                        break;

                    comp.Accumulator = 0;
                    comp.CurrState = SalvohudScanState.Active;
                    break;

                case SalvohudScanState.Active:
                    comp.Accumulator += frameTime;
                    comp.CurrRadius = comp.MaxRadius;

                    if (!(comp.Accumulator > comp.ActivePeriod))
                        break;

                    comp.Accumulator = 0;
                    comp.CurrState = SalvohudScanState.Idle;
                    break;
            }
        }

        //vaguely evil thing to get the salvohud the player is currently wearing
        _iconsComp = null;
        if (_playerMan.LocalEntity is not {} player || !TryComp<InventoryComponent>(player, out var inventoryComp))
            return;

        var invEnumerator = new InventorySystem.InventorySlotEnumerator(inventoryComp, SlotFlags.EYES);
        while (invEnumerator.MoveNext(out var inventorySlot))
        {
            if (inventorySlot.ContainedEntity != null && HasComp<ShowMaterialCompositionIconsComponent>(inventorySlot.ContainedEntity))
                _iconsComp = Comp<ShowMaterialCompositionIconsComponent>(inventorySlot.ContainedEntity.Value);
        }

        if (_iconsComp == null)
        {
            _overlay.Radius = 0f;
            _overlay.ScanPoint = null;
        }
        else
        {
            if (_iconsComp.CurrState == SalvohudScanState.In) //this feels like it sucks but kinda doesn't? idk but I kinda hate all of this code.
            {
                var edge0 = _iconsComp.InPeriod - _iconsComp.PingFadeoutTime;
                var edge1 = _iconsComp.InPeriod;
                _overlay.Alpha = 1f - (float) Math.Clamp((_iconsComp.Accumulator - edge0) / (edge1 - edge0), 0.0, 1.0);
            }
            else
            {
                _overlay.Alpha = 0f;
            }

            _overlay.Radius = _iconsComp.CurrRadius;
            _overlay.ScanPoint = _iconsComp.LastPingPos;
        }
    }

    private void OnGetStatusIconsEvent(Entity<PhysicalCompositionComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive || _iconsComp == null || _iconsComp.CurrState == SalvohudScanState.Idle || _iconsComp.LastPingPos == null)
            return;

        var diff = _xform.GetWorldPosition(entity) - _iconsComp.LastPingPos;
        var dist = diff.Value.Length();

        if (dist > _iconsComp.CurrRadius || dist < _iconsComp.CurrMinRadius)
            return;

        foreach (var (id, _) in entity.Comp.MaterialComposition.OrderByDescending(x => x.Value))
        {
            if (_protoMan.TryIndex<MaterialCompositionIconPrototype>(id, out var proto))
                args.StatusIcons.Add(proto);
        }
    }
}
