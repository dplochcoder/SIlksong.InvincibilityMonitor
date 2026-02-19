using Silksong.FsmUtil;
using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class DialogueCondition(InvincibilityMonitorPlugin plugin) : CallbackCondition(plugin)
{
    private readonly HashSet<PlayMakerFSM> activeShops = [];
    private readonly HashSet<PlayMakerFSM> activeTolls = [];

    private static readonly HashSet<string> boneBeastTravelStates = ["Open map", "Choose Scene", "Fade", "Hero Jump", "Hero Fire", "Jump Sing", "Time Passes", "Go To Stag Cutscene"];
    private readonly HashSet<PlayMakerFSM> boneBeasts = [];

    private static readonly HashSet<string> ventricaTravelStates = ["Interacted", "Hop In Antic", "Hop In", "Land In", "Open map", "Choose Scene", "Preload Scene", "Hero Press Button", "Close", "Leave", "Save State", "Fade Out", "Go To Next Scene"];
    private readonly HashSet<PlayMakerFSM> ventricas = [];

    public override string Key => "Dialogue";

    protected override string Description => "Whenever Hornet is engaged in any dialogue interaction.";

    protected override bool Callback() =>
        (QuestYesNoBox._instance != null && IsActive(QuestYesNoBox._instance.pane))
        || (QuestManager.instance != null && IsActive(QuestManager.instance))
        || (DialogueYesNoBox._instance != null && IsActive(DialogueYesNoBox._instance.pane))
        || (DialogueBox._instance != null && DialogueBox._instance.isDialogueRunning)
        || activeShops.Any(s => s.ActiveStateName != "Init" && s.ActiveStateName != "Idle")
        || activeTolls.Count > 0
        || boneBeasts.Any(b => boneBeastTravelStates.Contains(b.ActiveStateName))
        || ventricas.Any(v => ventricaTravelStates.Contains(v.ActiveStateName));

    private static bool IsActive(InventoryPaneBase? pane) => pane != null ? pane.IsPaneActive : false;

    private static bool IsActive(QuestManager qm) => qm.spawnedQuestAcceptedSequence.activeInHierarchy || qm.spawnedQuestFinishedSequence.activeInHierarchy;

    protected override void OnEnable()
    {
        base.OnEnable();
        Events.AddFsmEdit("FSM", EditTollFsm);
        Events.AddFsmEdit("shop_control", EditShopFsm);
        Events.AddFsmEdit("Bone Beast NPC", "Interaction", EditBoneBeastFsm);
        Events.AddFsmEdit("City Travel Tube", "Tube Travel", EditVentricaFsm);
    }

    protected override void OnDisable()
    {
        Events.RemoveFsmEdit("FSM", EditTollFsm);
        Events.RemoveFsmEdit("shop_control", EditShopFsm);
        Events.RemoveFsmEdit("Bone Beast NPC", "Interaction", EditBoneBeastFsm);
        Events.RemoveFsmEdit("City Travel Tube", "Tube Travel", EditVentricaFsm);
        base.OnDisable();
    }

    private void EditTollFsm(PlayMakerFSM fsm)
    {
        if (!fsm.HasStates(["Get Text", "Confirm", "Cancel", "Start Sequence", "Wait For Currency Counter", "Taking Currency", "Wait Frame", "Before Sequence Pause", "Keep Reach", "End"])) return;

        fsm.GetState("Start Sequence")!.AddMethod2(() => activeTolls.Add(fsm));
        fsm.GetState("End")!.InsertMethod2(0, () => activeTolls.Remove(fsm));
        fsm.gameObject.DoOnDestroy(() => activeTolls.Remove(fsm));
    }

    private void EditShopFsm(PlayMakerFSM fsm)
    {
        activeShops.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => activeShops.Remove(fsm));
    }

    private void EditBoneBeastFsm(PlayMakerFSM fsm)
    {
        boneBeasts.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => boneBeasts.Remove(fsm));
    }

    private void EditVentricaFsm(PlayMakerFSM fsm)
    {
        ventricas.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => ventricas.Remove(fsm));
    }
}
