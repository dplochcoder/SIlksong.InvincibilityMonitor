using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoDetour;
using MonoDetour.HookGen;
using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Conditions;

[MonoDetourTargets(typeof(CheckHeroPerformanceRegion))]
[MonoDetourTargets(typeof(CheckHeroPerformanceRegionV2))]
internal class NeedolinCondition : CallbackCondition
{
    private static NeedolinCondition? instance;

    internal NeedolinCondition(InvincibilityMonitorPlugin plugin) : base(plugin) => instance = this;

    public override string Key => "Needolin";

    protected override string Description => "Invincible while a background object is responding to the Needolin";

    private readonly HashSet<CheckHeroPerformanceRegion> checks = [];
    private readonly HashSet<CheckHeroPerformanceRegionV2> checksV2 = [];

    private static bool IsNone(FsmEvent? fsmEvent) => fsmEvent == null || fsmEvent.name == "";

    protected override bool Callback() => checks.Any(c => !IsNone(c.None) && c.active) || checksV2.Any(c => !IsNone(c.None) && c.active);

    private static void PostfixOnEnter(CheckHeroPerformanceRegion self)
    {
        instance?.checks.Add(self);
        self.Fsm.GameObject.DoOnDestroy(() => instance?.checks.Remove(self));
    }

    private static void PostfixOnEnter(CheckHeroPerformanceRegionV2 self)
    {
        instance?.checksV2.Add(self);
        self.Fsm.GameObject.DoOnDestroy(() => instance?.checksV2.Remove(self));
    }

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.HutongGames.PlayMaker.Actions.CheckHeroPerformanceRegion.OnEnter.Postfix(PostfixOnEnter);
        Md.HutongGames.PlayMaker.Actions.CheckHeroPerformanceRegionV2.OnEnter.Postfix(PostfixOnEnter);
    }
}
