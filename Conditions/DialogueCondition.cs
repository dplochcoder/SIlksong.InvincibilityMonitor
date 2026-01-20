using Silksong.InvincibilityMonitor.Util;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class DialogueCondition(InvincibilityMonitorPlugin plugin) : CallbackCondition(plugin)
{
    private readonly HashSet<PlayMakerFSM> activeShops = [];

    public override string Key => "Dialogue";

    protected override string Description => "Whenever Hornet is engaged in any dialogue interaction.";

    protected override bool Callback() =>
        (QuestYesNoBox._instance != null && IsActive(QuestYesNoBox._instance.pane))
        || (DialogueYesNoBox._instance != null && IsActive(DialogueYesNoBox._instance.pane))
        || (DialogueBox._instance != null && DialogueBox._instance.isDialogueRunning)
        || activeShops.Any(s => s.ActiveStateName != "Init" && s.ActiveStateName != "Idle");

    private static bool IsActive(InventoryPaneBase? pane) => pane != null ? pane.IsPaneActive : false;

    protected override void OnEnable()
    {
        base.OnEnable();
        // TODO: Edit fsms.
    }

    protected override void OnDisable()
    {
        // TODO: Edit fsms.
        base.OnDisable();
    }

    private void EditShopFsm(PlayMakerFSM fsm)
    {
        if (fsm.name != "shop_control") return;
        
        activeShops.Add(fsm);
        fsm.gameObject.DoOnDestroy(() => activeShops.Remove(fsm));
    }
}
