using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server._Impstation.DebrisAccumulator;

public sealed class DebrisAccumulatorSystem : EntitySystem
{
//I should probably write this down more, anyway
//have a constant 1~2 min long timer that pulls in new wrecks when it ticks over, to make it not be a *constant* stream of wrecks & allow for some downtime
//wreck group timer should be 5~7 mins
}
