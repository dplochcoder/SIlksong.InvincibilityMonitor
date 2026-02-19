using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Conditions;

internal abstract class InvincibilityCondition
{
    internal const string SECTION = "Conditions";

    protected readonly InvincibilityMonitorPlugin plugin;

    internal InvincibilityCondition(InvincibilityMonitorPlugin plugin)
    {
        this.plugin = plugin;

        var config = plugin.Config.Bind(
            configDefinition: new("Conditions", Key),
            defaultValue: false,
            configDescription: new(Description));

        IsConditionEnabled = config.Value;
        IsEnabled = plugin.PluginEnabled && IsConditionEnabled;

        config.SettingChanged += (_, args) =>
        {
            SettingChangedEventArgs typedArgs = (SettingChangedEventArgs)args;
            IsConditionEnabled = (bool)typedArgs.ChangedSetting.BoxedValue;
        };
        plugin.OnPluginEnabledChanged += _ => IsEnabled = plugin.PluginEnabled && IsConditionEnabled;

        if (IsEnabled) OnEnable();
    }

    public abstract string Key { get; }

    protected abstract string Description { get; }

    private void DoUpdate(Action modifier)
    {
        bool prevEnabled = IsEnabled;
        bool prevEnabledAndActive = IsEnabledAndActive;

        modifier();

        bool nextEnabled = IsEnabled;
        if (nextEnabled != prevEnabled)
        {
            if (nextEnabled) OnEnable();
            else OnDisable();
        }

        bool nextEnabledAndActive = IsEnabledAndActive;
        if (nextEnabledAndActive != prevEnabledAndActive) OnEnabledAndActiveChanged?.Invoke(nextEnabledAndActive);
    }

    protected bool IsEnabled
    {
        get => field;
        private set
        {
            if (field == value) return;
            DoUpdate(() => field = value);
        }
    }

    protected bool IsConditionEnabled
    {
        get => field;
        private set
        {
            if (field == value) return;
            DoUpdate(() => field = value);
        }
    }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() { }

    public bool IsEnabledAndActive => IsEnabled && Active;

    public bool Active
    {
        get => field;
        set
        {
            if (field == value) return;
            DoUpdate(() => field = value);
        }
    }

    // Invoked when IsEnabledAndActive changes.
    public event Action<bool>? OnEnabledAndActiveChanged;

    internal static List<InvincibilityCondition> CreateAllConditions(InvincibilityMonitorPlugin plugin) => [
        new AnimationCondition(plugin),
        new BenchCondition(plugin),
        new BossProgressionCondition(plugin),
        new CutsceneCondition(plugin),
        new DialogueCondition(plugin),
        new NeedolinCondition(plugin),
        new PickupCondition(plugin),
        new RoarLockCondition(plugin),
        new TransitioningCondition(plugin),
    ];
}
