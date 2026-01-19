using GlobalEnums;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class CutsceneCondition(InvincibilityMonitorPlugin plugin) : GameStateCondition(plugin, [GameState.CUTSCENE])
{
    public override string Key => "Cutscene";

    protected override string Description => "While any cutscene is playing.";
}
