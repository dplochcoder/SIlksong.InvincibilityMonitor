using GlobalEnums;
using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Conditions;

internal abstract class GameStateCondition(InvincibilityMonitorPlugin plugin, List<GameState> gameStates) : InvincibilityCondition(plugin)
{
    protected override void OnEnable()
    {
        Active = GameManager.instance != null && gameStates.Contains(GameManager.instance.GameState);

        LifecycleUtil.OnGameManagerAwake += OnGameManagerAwake;
        LifecycleUtil.OnGameManagerDestroy += OnGameManagerDestroy;
    }

    protected override void OnDisable()
    {
        if (GameManager.instance != null) GameManager.instance.GameStateChange -= GameStateChange;
        LifecycleUtil.OnGameManagerAwake -= OnGameManagerAwake;
        LifecycleUtil.OnGameManagerDestroy -= OnGameManagerDestroy;

        Active = false;
    }

    private void OnGameManagerAwake(GameManager gameManager)
    {
        Active = gameStates.Contains(gameManager.GameState);
        gameManager.GameStateChange += GameStateChange;
    }

    private void OnGameManagerDestroy(GameManager gameManager) => Active = false;

    private void GameStateChange(GameState gameState) => Active = gameStates.Contains(gameState);
}
