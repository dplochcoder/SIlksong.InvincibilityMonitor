namespace Silksong.InvincibilityMonitor.Conditions;

internal abstract class CallbackCondition(InvincibilityMonitorPlugin plugin) : InvincibilityCondition(plugin)
{
    protected abstract bool Callback();

    private void InvokeCallback() => Active = Callback();

    protected override void OnEnable()
    {
        InvokeCallback();
        plugin.OnUpdate += InvokeCallback;
    }

    protected override void OnDisable()
    {
        Active = false;
        plugin.OnUpdate -= InvokeCallback;
    }
}
