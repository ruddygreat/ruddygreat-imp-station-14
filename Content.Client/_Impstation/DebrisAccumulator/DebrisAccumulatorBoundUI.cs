using System.Linq;
using Content.Client._Impstation.DebrisAccumulator.UI;
using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Content.Shared.Salvage;
using Content.Shared.Salvage.Magnet;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Serilog;

namespace Content.Client._Impstation.DebrisAccumulator;

public sealed class DebrisAccumulatorBoundUserInterface : BoundUserInterface
{

    private DebrisAccumulatorWindow? _window;

    public DebrisAccumulatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DebrisAccumulatorWindow>();
        _window.Title = "Debris accumulator";
        _window.OpenCenteredLeft();

        var group = new DebrisAccumulatorWreckGroup();
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        _window.AddGroup(group);
        group = new DebrisAccumulatorWreckGroup();
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        _window.AddGroup(group);
        group = new DebrisAccumulatorWreckGroup();
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        group.AddWreckInfo(new DebrisAccumulatorWreckInfoDisplay());
        _window.AddGroup(group);
    }
}
