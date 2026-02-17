using GlobalEnums;
using MonoDetour;
using MonoDetour.HookGen;
using Silksong.PurenailUtil.Collections;
using System;

namespace Silksong.InvincibilityMonitor.Util;

[MonoDetourTargets(typeof(GameManager))]
[MonoDetourTargets(typeof(PlayMakerFSM))]
internal static class Events
{
    private static readonly HashMultimap<string, Action<PlayMakerFSM>> fsmEditsByName = [];
    private static readonly HashMultitable<string, string, Action<PlayMakerFSM>> fsmEditsByObjAndName = [];

    internal static void AddFsmEdit(string fsmName, Action<PlayMakerFSM> edit) => fsmEditsByName.Add(fsmName, edit);

    internal static void AddFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit) => fsmEditsByObjAndName.Add(objName, fsmName, edit);

    internal static void RemoveFsmEdit(string fsmName, Action<PlayMakerFSM> edit) => fsmEditsByName.Remove(fsmName, edit);

    internal static void RemoveFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit) => fsmEditsByObjAndName.Remove(objName, fsmName, edit);

    private static void OnEnable(PlayMakerFSM fsm)
    {
        foreach (var action in fsmEditsByName.Get(fsm.FsmName)) action(fsm);
        foreach (var action in fsmEditsByObjAndName.Get(fsm.gameObject.name, fsm.FsmName)) action(fsm);
    }

    internal static event Action<GameState>? OnGameStateChanged;

    private static void WatchGameStateChange(GameManager gameManager)
    {
        OnGameStateChanged?.Invoke(gameManager.GameState);
        gameManager.GameStateChange += state => OnGameStateChanged?.Invoke(state);
    }

    internal static event Action? OnNextLevelReady;

    private static void WatchOnNextLevelReady(GameManager gameManager) => OnNextLevelReady?.Invoke();

    static Events() => LifecycleUtil.OnGameManagerAwake += WatchGameStateChange;

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.GameManager.OnNextLevelReady.Postfix(WatchOnNextLevelReady);
        Md.PlayMakerFSM.OnEnable.Postfix(OnEnable);
    }
}
