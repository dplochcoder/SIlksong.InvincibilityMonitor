using GlobalEnums;
using MonoDetour;
using MonoDetour.HookGen;
using System;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Util;

[MonoDetourTargets(typeof(GameManager))]
[MonoDetourTargets(typeof(PlayMakerFSM))]
internal static class Events
{
    private static readonly Dictionary<string, HashSet<Action<PlayMakerFSM>>> fsmEditsByName = [];
    private static readonly Dictionary<string, Dictionary<string, HashSet<Action<PlayMakerFSM>>>> fsmEditsByObjAndName = [];

    internal static void AddFsmEdit(string fsmName, Action<PlayMakerFSM> edit)
    {
        if (!fsmEditsByName.TryGetValue(fsmName, out var set)) fsmEditsByName.Add(fsmName, [edit]);
        else set.Add(edit);
    }

    internal static void AddFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit)
    {
        if (!fsmEditsByObjAndName.TryGetValue(objName, out var dict))
        {
            dict = [];
            fsmEditsByObjAndName.Add(objName, dict);
        }

        if (!dict.TryGetValue(fsmName, out var set)) dict.Add(fsmName, [edit]);
        else set.Add(edit);
    }

    internal static void RemoveFsmEdit(string fsmName, Action<PlayMakerFSM> edit)
    {
        if (fsmEditsByName.TryGetValue(fsmName, out var set) && set.Remove(edit) && set.Count == 0) fsmEditsByName.Remove(fsmName);
    }

    internal static void RemoveFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit)
    {
        if (fsmEditsByObjAndName.TryGetValue(objName, out var dict))
        {
            if (dict.TryGetValue(fsmName, out var set) && set.Remove(edit) && set.Count == 0)
            {
                dict.Remove(fsmName);
                if (dict.Count == 0) fsmEditsByObjAndName.Remove(objName);
            }
        }
    }

    private static void OnEnable(PlayMakerFSM fsm)
    {
        if (fsmEditsByName.TryGetValue(fsm.FsmName, out var actions))
        {
            foreach (var action in actions) action(fsm);
        }

        if (fsmEditsByObjAndName.TryGetValue(fsm.gameObject.name, out var dict) && dict.TryGetValue(fsm.FsmName, out actions))
        {
            foreach (var action in actions) action(fsm);
        }
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
