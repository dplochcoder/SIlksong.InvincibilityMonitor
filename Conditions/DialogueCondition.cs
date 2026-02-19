using MonoDetour;
using MonoDetour.HookGen;
using Silksong.FsmUtil;
using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Conditions;

[MonoDetourTargets(typeof(QuestItemBoard))]
internal class DialogueCondition : CallbackCondition
{
    private static DialogueCondition? instance;

    internal DialogueCondition(InvincibilityMonitorPlugin plugin) : base(plugin) => instance = this;

    private readonly HashSet<PlayMakerFSM> activeShops = [];
    private readonly HashSet<PlayMakerFSM> activeTolls = [];
    private readonly HashSet<QuestItemBoard> questBoards = [];
    private readonly HashSet<PlayMakerFSM> questBoardFsms = [];

    private static readonly HashSet<string> boneBeastTravelStates = ["Open map", "Choose Scene", "Fade", "Hero Jump", "Hero Fire", "Jump Sing", "Time Passes", "Go To Stag Cutscene"];
    private readonly HashSet<PlayMakerFSM> boneBeastFsms = [];

    private static readonly HashSet<string> ventricaTravelStates = ["Interacted", "Hop In Antic", "Hop In", "Land In", "Open map", "Choose Scene", "Preload Scene", "Hero Press Button", "Close", "Leave", "Save State", "Fade Out", "Go To Next Scene"];
    private readonly HashSet<PlayMakerFSM> ventricaFsms = [];

    public override string Key => "Dialogue";

    protected override string Description => "Whenever Hornet is engaged in any dialogue interaction.";

    protected override bool Callback() =>
        (QuestYesNoBox._instance != null && IsActive(QuestYesNoBox._instance.pane))
        || (QuestManager.instance != null && IsActive(QuestManager.instance))
        || (DialogueYesNoBox._instance != null && IsActive(DialogueYesNoBox._instance.pane))
        || (DialogueBox._instance != null && DialogueBox._instance.isDialogueRunning)
        || activeShops.Any(s => s.Active && s.ActiveStateName != "Init" && s.ActiveStateName != "Idle")
        || activeTolls.Count > 0
        || questBoards.Any(b => IsActive(b.pane))
        || questBoardFsms.Any(f => f.Active && f.ActiveStateName != "Idle")
        || boneBeastFsms.Any(b => boneBeastTravelStates.Contains(b.ActiveStateName))
        || ventricaFsms.Any(v => ventricaTravelStates.Contains(v.ActiveStateName));

#pragma warning disable IDE0075 // Cannot simplify because it's a Unity object.
    private static bool IsActive(InventoryPaneBase? pane) => pane != null ? pane.IsPaneActive : false;
#pragma warning restore IDE0075

    private static bool IsActive(QuestManager qm) => qm.spawnedQuestAcceptedSequence.activeInHierarchy || qm.spawnedQuestFinishedSequence.activeInHierarchy;

    protected override void OnEnable()
    {
        base.OnEnable();
        Events.AddFsmEdit("FSM", EditTollFsm);
        Events.AddFsmEdit("shop_control", EditShopFsm);
        Events.AddFsmEdit("Quest Board", "Hand In Sequence", EditQuestBoardFsm);
        Events.AddFsmEdit("Bone Beast NPC", "Interaction", EditBoneBeastFsm);
        Events.AddFsmEdit("City Travel Tube", "Tube Travel", EditVentricaFsm);
    }

    protected override void OnDisable()
    {
        Events.RemoveFsmEdit("FSM", EditTollFsm);
        Events.RemoveFsmEdit("shop_control", EditShopFsm);
        Events.RemoveFsmEdit("Quest Board", "Hand In Sequence", EditQuestBoardFsm);
        Events.RemoveFsmEdit("Bone Beast NPC", "Interaction", EditBoneBeastFsm);
        Events.RemoveFsmEdit("City Travel Tube", "Tube Travel", EditVentricaFsm);
        base.OnDisable();
    }

    private void EditTollFsm(PlayMakerFSM fsm)
    {
        if (!fsm.HasStates(["Get Text", "Confirm", "Cancel", "Start Sequence", "Wait For Currency Counter", "Taking Currency", "Wait Frame", "Before Sequence Pause", "Keep Reach", "End"])) return;

        fsm.GetState("Start Sequence")!.AddMethod(() => activeTolls.Add(fsm));
        fsm.GetState("End")!.InsertMethod(0, () => activeTolls.Remove(fsm));
        fsm.gameObject.DoOnDestroy(() => activeTolls.Remove(fsm));
    }

    private void EditShopFsm(PlayMakerFSM fsm)
    {
        activeShops.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => activeShops.Remove(fsm));
    }

    private void EditQuestBoardFsm(PlayMakerFSM fsm)
    {
        questBoardFsms.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => questBoardFsms.Remove(fsm));
    }

    private void EditBoneBeastFsm(PlayMakerFSM fsm)
    {
        boneBeastFsms.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => boneBeastFsms.Remove(fsm));
    }

    private void EditVentricaFsm(PlayMakerFSM fsm)
    {
        ventricaFsms.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => ventricaFsms.Remove(fsm));
    }

    private static void PrefixQuestBoardAwake(QuestItemBoard self)
    {
        instance?.questBoards.Add(self);
        self.gameObject.DoOnDestroy(() => instance?.questBoards.Remove(self));
    }

    [MonoDetourHookInitialize]
    private static void Hook() => Md.QuestItemBoard.Awake.Prefix(PrefixQuestBoardAwake);
}
