using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Impstation.SalvoHud;

public sealed class SalvoHudScanOverlay : Overlay
{

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public Vector2? ScanPoint = null;
    public float Radius = 0;
    public float Alpha = 1f;

    private ShaderInstance _shader;

    public SalvoHudScanOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _prototypeManager.Index<ShaderPrototype>("SalvoScan").InstanceUnique();
    }

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowEntities;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScanPoint == null)
            return;

        var worldHandle = args.WorldHandle;

        var bl = new Vector2(ScanPoint.Value.X - Radius, ScanPoint.Value.Y - Radius);
        var tr = new Vector2(ScanPoint.Value.X + Radius, ScanPoint.Value.Y + Radius);

        _shader.SetParameter("FADE_IN_DIST", 0.1f);
        _shader.SetParameter("ALPHA", Alpha);

        worldHandle.UseShader(_shader);
        worldHandle.DrawRect(new Box2(bl, tr), Color.White);
        worldHandle.UseShader(null);
    }
}
