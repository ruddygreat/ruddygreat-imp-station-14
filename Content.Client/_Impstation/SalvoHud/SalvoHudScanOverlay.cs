using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client._Impstation.SalvoHud;

public sealed class SalvoHudScanOverlay : Overlay
{

    public Vector2? ScanPoint = null;
    public float MinRadius = 0;
    public float MaxRadius = 0;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowEntities;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScanPoint == null)
            return;

        var worldHandle = args.WorldHandle;

        //todo make this look good
        worldHandle.DrawCircle(ScanPoint.Value, MaxRadius, Color.Blue);
        worldHandle.DrawCircle(ScanPoint.Value, MinRadius, Color.Red);
    }
}
