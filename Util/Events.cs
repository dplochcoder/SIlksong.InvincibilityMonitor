using GlobalEnums;
using HutongGames.PlayMaker;
using MonoDetour;
using MonoDetour.HookGen;
using Silksong.PurenailUtil.Collections;
using System;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Util;

[MonoDetourTargets(typeof(Fsm))]
[MonoDetourTargets(typeof(GameManager))]
[MonoDetourTargets(typeof(PlayMakerFSM))]
internal static class Events
{
    private static readonly HashSet<Action<Fsm>> rawFsmEdits = [];
    private static readonly HashMultimap<string, Action<PlayMakerFSM>> fsmEditsByName = [];
    private static readonly HashMultitable<string, string, Action<PlayMakerFSM>> fsmEditsByObjAndName = [];

    internal static void AddRawFsmEdit(Action<Fsm> fsmEdit) => rawFsmEdits.Add(fsmEdit);
    internal static void AddFsmEdit(string fsmName, Action<PlayMakerFSM> fsmEdit) => fsmEditsByName.Add(fsmName, fsmEdit);
    internal static void AddFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> fsmEdit) => fsmEditsByObjAndName.Add(objName, fsmName, fsmEdit);

    internal static void RemoveRawFsmEdit(Action<Fsm> fsmEdit) => rawFsmEdits.Remove(fsmEdit);
    internal static void RemoveFsmEdit(string fsmName, Action<PlayMakerFSM> fsmEdit) => fsmEditsByName.Remove(fsmName, fsmEdit);
    internal static void RemoveFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> fsmEdit) => fsmEditsByObjAndName.Remove(objName, fsmName, fsmEdit);

    private static void OnEnable(Fsm fsm)
    {
        foreach (var action in rawFsmEdits) action(fsm);
    }

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
        Md.HutongGames.PlayMaker.Fsm.OnEnable.Postfix(OnEnable);
        Md.GameManager.OnNextLevelReady.Postfix(WatchOnNextLevelReady);
        Md.PlayMakerFSM.OnEnable.Postfix(OnEnable);
    }
}
