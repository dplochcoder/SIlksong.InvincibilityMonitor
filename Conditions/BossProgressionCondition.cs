using Silksong.FsmUtil;
using Silksong.InvincibilityMonitor.Util;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class BossProgressionCondition(InvincibilityMonitorPlugin plugin) : InvincibilityCondition(plugin)
{
    public override string Key => "Boss Progression";

    protected override string Description => "After last-hitting Widow, the First Sinner, Grandmother Silk, or Lost Lace.";

    protected override void OnEnable()
    {
        base.OnEnable();

        Events.OnNextLevelReady += SetInactive;
        Events.AddFsmEdit("Spinner Boss", "Control", EditWeaver);
        Events.AddFsmEdit("First Weaver", "Control", EditWeaver);
        Events.AddFsmEdit("Silk Boss", "Phase Control", EditGMS);
        Events.AddFsmEdit("Lost Lace Boss", "Death Control", EditLostLace);
    }

    protected override void OnDisable()
    {
        Events.OnNextLevelReady -= SetInactive;
        Events.RemoveFsmEdit("Spinner Boss", "Control", EditWeaver);
        Events.RemoveFsmEdit("First Weaver", "Control", EditWeaver);
        Events.RemoveFsmEdit("Silk Boss", "Phase Control", EditGMS);
        Events.RemoveFsmEdit("Lost Lace Boss", "Death Control", EditLostLace);

        base.OnDisable();
    }

    // belltown_shrine
    private void EditWeaver(PlayMakerFSM fsm) => fsm.GetState("Death Stagger")?.InsertMethod2(0, () => Active = true);

    // cradle_03
    private void EditGMS(PlayMakerFSM fsm) => fsm.GetState("Death Hit")!.InsertMethod2(0, () => Active = true);

    // abyss_coccoon
    private void EditLostLace(PlayMakerFSM fsm) => fsm.GetState("Allow Death")!.InsertMethod2(0, () => Active = true);

    private void SetInactive() => Active = false;
}
