using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;
using Silksong.InvincibilityMonitor.Util;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class EndgameCondition(InvincibilityMonitorPlugin plugin) : InvincibilityCondition(plugin)
{
    public override string Key => "Endgame";

    protected override string Description => "After last-hitting Grandmother Silk or Lost Lace.";

    protected override void OnEnable()
    {
        base.OnEnable();

        Events.OnNextLevelReady += SetInactive;
        Events.AddFsmEdit("Silk Boss", "Phase Control", EditGMS);
        Events.AddFsmEdit("Lost Lace Boss", "Death Control", EditLostLace);
    }

    protected override void OnDisable()
    {
        Events.OnNextLevelReady -= SetInactive;
        Events.RemoveFsmEdit("Silk Boss", "Phase Control", EditGMS);
        Events.RemoveFsmEdit("Lost Lace Boss", "Death Control", EditLostLace);

        base.OnDisable();
    }

    // cradle_03
    protected void EditGMS(PlayMakerFSM fsm) => fsm.GetState("Death Hit")!.InsertAction(0, new LambdaAction()
    {
        Method = () => Active = true
    });

    // abyss_coccoon
    protected void EditLostLace(PlayMakerFSM fsm) => fsm.GetState("Allow Death")!.InsertAction(0, new LambdaAction()
    {
        Method = () => Active = true
    });

    private void SetInactive() => Active = false;
}
