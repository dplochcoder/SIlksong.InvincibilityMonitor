using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;
using Silksong.InvincibilityMonitor.Util;
using System;
using UnityEngine;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class RoarLockCondition(InvincibilityMonitorPlugin plugin) : InvincibilityCondition(plugin)
{
    public override string Key => "Roar Lock";

    protected override string Description => "When Hornet is stunned by a boss's roar animation.";

    protected override void OnEnable() => LifecycleUtil.OnHeroControllerAwake += OnHeroControllerAwake;

    protected override void OnDisable()
    {
        LifecycleUtil.OnHeroControllerAwake -= OnHeroControllerAwake;

        if (HeroController.instance != null && HeroController.instance.gameObject != null && HeroController.instance.gameObject.TryGetComponent<RoarLockMonitor>(out var monitor))
            UnityEngine.Object.Destroy(monitor);
        Active = false;
    }

    private void OnHeroControllerAwake(HeroController heroController) => heroController.gameObject.AddComponent<RoarLockMonitor>().SetRoarLocked = value => Active = value;
}

internal class RoarLockMonitor : MonoBehaviour
{
    private PlayMakerFSM? roarFsm;
    private int roarLockedForFrames;

    internal Action<bool>? SetRoarLocked;

    private void Update()
    {
        if (roarFsm == null)
        {
            roarFsm = gameObject.LocateMyFSM("Roar and Wound States");
            if (roarFsm == null) return;

            roarFsm.GetState("Roar Lock Start")?.InsertAction(new LambdaAction()
            {
                Method = () =>
                {
                    roarLockedForFrames = 1;
                    SetRoarLocked?.Invoke(true);
                }
            }, 0);
            return;
        }

        if (roarLockedForFrames == 0) return;
        if (++roarLockedForFrames > 2 && HeroController.instance.acceptingInput)
        {
            roarLockedForFrames = 0;
            SetRoarLocked?.Invoke(false);
        }
    }
}
