using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Util;

internal static class FSMExtensions
{
    internal static bool HasStates(this PlayMakerFSM fsm, IEnumerable<string> states)
    {
        HashSet<string> owned = [.. fsm.FsmStates.Select(s => s.Name)];
        return states.All(owned.Contains);
    }
}
