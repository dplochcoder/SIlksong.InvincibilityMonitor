namespace Silksong.InvincibilityMonitor.Conditions;

internal class BenchCondition(InvincibilityMonitorPlugin plugin) : InvincibilityCondition(plugin)
{
    public override string Key => "Bench";

    protected override string Description => "Invincible when resting at a bench.";

    protected override void OnEnable()
    {
        Active = PlayerData.instance?.atBench ?? false;
        PrepatcherPlugin.PlayerDataVariableEvents<bool>.OnSetVariable += SetAtBench;
    }

    protected override void OnDisable()
    {
        PrepatcherPlugin.PlayerDataVariableEvents<bool>.OnSetVariable -= SetAtBench;
        Active = false;
    }

    private bool SetAtBench(PlayerData playerData, string name, bool value)
    {
        if (name == nameof(PlayerData.atBench)) Active = value;
        return value;
    }
}
