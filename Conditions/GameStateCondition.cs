using GlobalEnums;
using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Conditions;

internal abstract class GameStateCondition(InvincibilityMonitorPlugin plugin, List<GameState> gameStates) : InvincibilityCondition(plugin)
{
    protected override void OnEnable()
    {
        Active = GameManager.instance != null && gameStates.Contains(GameManager.instance.GameState);

        Events.OnGameStateChanged += OnGameStateChanged;
        LifecycleUtil.OnGameManagerDestroy += OnGameManagerDestroy;
    }

    protected override void OnDisable()
    {
        Events.OnGameStateChanged -= OnGameStateChanged;
        LifecycleUtil.OnGameManagerDestroy -= OnGameManagerDestroy;

        Active = false;
    }

    private void OnGameManagerDestroy(GameManager gameManager) => Active = false;

    private void OnGameStateChanged(GameState gameState) => Active = gameStates.Contains(gameState);
}
