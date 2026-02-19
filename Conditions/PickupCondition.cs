using MonoDetour;
using MonoDetour.HookGen;
using Silksong.InvincibilityMonitor.Util;
using System.Collections;

namespace Silksong.InvincibilityMonitor.Conditions;

[MonoDetourTargets(typeof(CollectableItemPickup))]
internal class PickupCondition : InvincibilityCondition
{
    private static PickupCondition? instance;

    internal PickupCondition(InvincibilityMonitorPlugin plugin) : base(plugin) => instance = this;

    public override string Key => "Pickup";

    protected override string Description => "Make Hornet invincible while grabbing shiny pickups";

    private void ModifyPickup(CollectableItemPickup pickup, ref IEnumerator routine)
    {
        var copy = routine;
        IEnumerator Wrapped()
        {
            Active = true;
            while (copy.MoveNext()) yield return copy.Current;
            Active = false;
        }

        pickup.gameObject.DoOnDestroy(() => Active = false);
        routine = Wrapped();
    }

    private static void PostfixPickup(CollectableItemPickup self, ref IEnumerator returnValue) => instance?.ModifyPickup(self, ref returnValue);

    [MonoDetourHookInitialize]
    private static void Hook() => Md.CollectableItemPickup.Pickup.Postfix(PostfixPickup);
}
