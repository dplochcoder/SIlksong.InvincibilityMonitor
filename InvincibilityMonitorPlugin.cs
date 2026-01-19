using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Silksong.InvincibilityMonitor.Conditions;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Silksong.InvincibilityMonitor;

[BepInDependency("org.silksong-modding.fsmutil")]
[BepInDependency("org.silksong-modding.prepatcher")]
[BepInAutoPlugin(id: "io.github.invincibilitymonitor")]
public partial class InvincibilityMonitorPlugin : BaseUnityPlugin, IModMenuCustomMenu
{
    private ConfigEntry<bool>? pluginEnabledConfig;
    internal bool PluginEnabled => pluginEnabledConfig?.Value ?? false;
    internal event Action<bool>? OnPluginEnabledChanged;

    private ConfigEntry<float>? gracePeriodConfig;

    private readonly List<string> conditionNames = [];
    private readonly List<bool> activeConditions = [];
    private int numActiveConditions = 0;

    private void Awake()
    {
        pluginEnabledConfig = Config.Bind(configDefinition: new("Main", "Enabled"),
            false,
            configDescription: new("Whether to apply any invincibility rules at all."));
        pluginEnabledConfig.SettingChanged += (_, args) =>
        {
            SettingChangedEventArgs typedArgs = (SettingChangedEventArgs)args;
            OnPluginEnabledChanged?.Invoke((bool)typedArgs.ChangedSetting.BoxedValue);
        };

        gracePeriodConfig = Config.Bind(configDefinition: new("Main", "Grace Period"),
            0.2f,
            new("Time (seconds) for invincibility to wear off.", tags: [(ConfigEntryFactory.MenuElementGenerator)CreateGracePeriodElement]));

        List<Type> types = [.. typeof(InvincibilityMonitorPlugin).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(InvincibilityCondition)) && !t.IsAbstract).OrderBy(t => t.Name)];
        foreach (var type in types)
        {
            int index = activeConditions.Count;
            activeConditions.Add(false);

            InvincibilityCondition condition = (InvincibilityCondition)type.GetConstructor([typeof(InvincibilityMonitorPlugin)]).Invoke([this]);
            conditionNames.Add(condition.Key);

            void OnChange(bool value)
            {
                bool prev = activeConditions[index];
                if (prev == value) return;

                activeConditions[index] = value;
                if (value) ++numActiveConditions;
                else --numActiveConditions;
            }

            OnChange(condition.IsEnabledAndActive);
            condition.OnEnabledAndActiveChanged += OnChange;
        }

        if (Chainloader.PluginInfos.ContainsKey("io.github.hk-speedrunning.debugmod")) HookDebugMod();

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    public string ModMenuName() => "Invincibility Monitor";

    public AbstractMenuScreen BuildCustomMenu()
    {
        SimpleMenuScreen screen = new("Invincibility Monitor");
        PaginatedMenuScreenBuilder conditionsBuilder = new("Invincibility Conditions");

        ConfigEntryFactory factory = new();
        foreach (var (configDefinition, configBase) in Config)
        {
            if (!factory.GenerateMenuElement(configBase, out var menuElement)) continue;

            if (configDefinition.Section == InvincibilityCondition.SECTION) conditionsBuilder.Add(menuElement);
            else screen.Add(menuElement);
        }

        TextButton subMenu = new("Conditions");
        var conditions = conditionsBuilder.Build();
        subMenu.OnSubmit += () => MenuScreenNavigation.Show(conditions);
        screen.Add(subMenu);

        return screen;
    }

    private static bool CreateGracePeriodElement(ConfigEntryBase entry, out MenuElement menuElement)
    {
        ListSliderModel<float> model = new([0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f]) { DisplayFn = (idx, _) => $"{(idx / 10.0f):0.0}" };
        SliderElement<float> sliderElement = new("Grace Period", model);
        sliderElement.SynchronizeRawWith(entry);

        menuElement = sliderElement;
        return true;
    }

    private void HookDebugMod() => DebugMod.DebugMod.AddTextToInfoPanel("Invincible", () => IsCurrentlyInvincible ? "Yes" : "No");

    private void OnEnable() => PrepatcherPlugin.PlayerDataVariableEvents<bool>.OnGetVariable += OverrideIsInvincible;

    private void OnDisable()
    {
        invincibilityCooldown = 0f;
        PrepatcherPlugin.PlayerDataVariableEvents<bool>.OnGetVariable -= OverrideIsInvincible;
    }

    private bool HasInvincibilityCondition => PluginEnabled && numActiveConditions > 0;

    private bool IsCurrentlyInvincible => HasInvincibilityCondition || invincibilityCooldown > 0f;

    private float invincibilityCooldown = 0f;  // Cooldown before invincibility goes away.

    private void Update()
    {
        if (HasInvincibilityCondition) invincibilityCooldown = gracePeriodConfig?.Value ?? 0;
        else if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Time.deltaTime;
            if (invincibilityCooldown < 0) invincibilityCooldown = 0;
        }
    }

    private bool OverrideIsInvincible(PlayerData playerData, string name, bool orig) => orig || (name == nameof(PlayerData.isInvincible) && IsCurrentlyInvincible);
}
