using GlobalEnums;
using MonoDetour;
using MonoDetour.HookGen;
using System;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Util;

[MonoDetourTargets(typeof(GameManager))]
[MonoDetourTargets(typeof(HeroController))]
internal static class LifecycleUtil
{
    private static readonly HashSet<Action<GameManager>> gmAwakeActions = [];

    internal static event Action<GameManager> OnGameManagerAwake
    {
        add {
            gmAwakeActions.Add(value);
            if (GameManager.instance != null) value(GameManager.instance);
        }
        remove => gmAwakeActions.Remove(value);
    }

    internal static event Action<GameManager>? OnGameManagerDestroy;

    private static readonly HashSet<Action<HeroController>> hcActions = [];

    internal static event Action<HeroController> OnHeroControllerAwake
    {
        add
        {
            hcActions.Add(value);
            if (HeroController.instance != null) value(HeroController.instance);
        }
        remove => hcActions.Remove(value);
    }

    internal static event Action<HeroController>? OnHeroControllerDestroy;

    private static void GameManagerPostfixAwake(GameManager self)
    {
        List<Action<GameManager>> actions = [.. gmAwakeActions];
        foreach (var action in actions) action(self);
    }

    private static void GameManagerPostfixDestroy(GameManager self) => OnGameManagerDestroy?.Invoke(self);

    private static void HeroControllerPostfixAwake(HeroController self)
    {
        List<Action<HeroController>> actions = [.. hcActions];
        foreach (var action in actions) action(self);
    }

    private static void HeroControllerPostfixDestroy(HeroController self) => OnHeroControllerDestroy?.Invoke(self);

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.GameManager.Awake.Postfix(GameManagerPostfixAwake);
        Md.GameManager.OnDestroy.Postfix(GameManagerPostfixDestroy);
        Md.HeroController.Awake.Postfix(HeroControllerPostfixAwake);
        Md.HeroController.OnDestroy.Postfix(HeroControllerPostfixDestroy);
    }
}
