using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.SalvoHud;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShowMaterialCompositionIconsComponent : Component
{
    [DataField]
    public EntProtoId ActivateActionProtoID = "ActionActivateSalvoHud";

    [DataField, AutoNetworkedField]
    public EntityUid? ActivateActionEnt;

    [DataField]
    public Vector2? LastPingPos = null;

    /// <summary>
    /// how long the current scan has been active for
    /// </summary>
    [DataField]
    public float Accumulator = 0;

    /// <summary>
    /// the current state of the scan
    /// </summary>
    [DataField]
    public SalvohudScanState CurrState = SalvohudScanState.Idle;

    /// <summary>
    /// how long to stay in the "in" state
    /// </summary>
    [DataField]
    public float InPeriod = 1;

    /// <summary>
    /// how long to stay in the "active" state
    /// </summary>
    [DataField]
    public float ActivePeriod = 8;

    /// <summary>
    /// how long to stay in the "out" state
    /// </summary>
    [DataField]
    public float OutPeriod = 1;

    /// <summary>
    /// max range for the scan
    /// </summary>
    [DataField]
    public float MaxRange = 10;

    /// <summary>
    /// the current minimum range. set during out.
    /// </summary>
    [DataField]
    public float CurrMinRange = 0;

    /// <summary>
    /// the current max range. set during in.
    /// </summary>
    [DataField]
    public float CurrMaxRange = 0;
}

public enum SalvohudScanState
{
    Idle,
    In,
    Active,
    Out
}
