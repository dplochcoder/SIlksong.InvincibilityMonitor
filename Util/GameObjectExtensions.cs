using Silksong.UnityHelper.Extensions;
using System;
using UnityEngine;

namespace Silksong.InvincibilityMonitor.Util;

internal static class GameObjectExtensions
{
    internal static void DoOnDestroy(this GameObject self, Action action) => self.GetOrAddComponent<OnDestroyHelper>().Event += action;
}

internal class OnDestroyHelper : MonoBehaviour
{
    internal event Action? Event;

    private void OnDestroy() => Event?.Invoke();
}
