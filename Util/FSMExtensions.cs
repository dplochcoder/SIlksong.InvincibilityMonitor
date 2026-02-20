using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Util;

internal static class FSMExtensions
{
    internal static bool HasStates(this Fsm fsm, IEnumerable<string> states)
    {
        HashSet<string> owned = [.. fsm.states.Select(s => s.Name)];
        return states.All(owned.Contains);
    }
}
