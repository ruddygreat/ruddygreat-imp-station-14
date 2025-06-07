using Content.Shared.Actions;
using Content.Shared.Inventory;
using Robust.Shared.Timing;

namespace Content.Shared._Impstation.SalvoHud;

/// <summary>
/// This handles adding the toggle salvohud actions to the player when they wear it
/// </summary>
public sealed class SharedSalvohudToggleSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShowMaterialCompositionIconsComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<ShowMaterialCompositionIconsComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<ShowMaterialCompositionIconsComponent, ActivateSalvoHudEvent>(OnActivateSalvoHudEvent);
    }

    private void OnActivateSalvoHudEvent(Entity<ShowMaterialCompositionIconsComponent> ent, ref ActivateSalvoHudEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        ent.Comp.CurrState = SalvohudScanState.In;
        ent.Comp.LastPingPos = _xform.GetWorldPosition(ent);
        args.Handled = true;
    }

    private void OnMapInitEvent(Entity<ShowMaterialCompositionIconsComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActivateActionEnt, ent.Comp.ActivateActionProtoID);
    }

    private void OnGetItemActions(Entity<ShowMaterialCompositionIconsComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags == null)
            return;

        args.AddAction(ref ent.Comp.ActivateActionEnt, ent.Comp.ActivateActionProtoID);
    }
}
