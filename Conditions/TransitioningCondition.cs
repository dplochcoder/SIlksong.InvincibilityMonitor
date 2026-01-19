using GlobalEnums;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class TransitioningCondition(InvincibilityMonitorPlugin plugin) : GameStateCondition(plugin, [GameState.ENTERING_LEVEL, GameState.EXITING_LEVEL])
{
    public override string Key => "Transitioning";

    protected override string Description => "While entering or exiting a level.";
}
