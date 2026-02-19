using System.Collections.Generic;

namespace Silksong.InvincibilityMonitor.Conditions;

internal class AnimationCondition(InvincibilityMonitorPlugin plugin) : CallbackCondition(plugin)
{
    public override string Key => "Animation";

    protected override string Description => "Invincible when in any one of various dialogue or recovery animations.";

    // For full list, in UEPlus run:
    // string.Join("\",\n\"", HeroController.instance.gameObject.GetComponent<tk2dSpriteAnimator>().Library.clips.Select(c => c.name).Where(n => n != "").OrderBy(n => n));
    private static readonly HashSet<string> CLIPS =
    [
        "Abyss Kneel Back Idle",
        "Abyss Kneel Back Talk",
        "Abyss Kneel Idle",
        "Abyss Kneel to Stand",
        "Abyss Kneel Turn Back",
        "Challenge Strong",
        "Challenge Talk",
        "Challenge Talk End",
        "Challenge Talk End ToIdle",
        "Challenge Talk End ToTalk",
        "Challenge Talk Idle",
        "Challenge Talk Idle Start",
        "Challenge Talk Start",
        "ChallengeStrongToIdle",
        "Collect Heart Piece",
        "Collect Heart Piece End",
        "Collect Memory Orb",
        "Collect Normal 1",
        "Collect Normal 1 Q",
        "Collect Normal 2",
        "Collect Normal 3",
        "Collect Normal 3 Q",
        "Collect Silk Heart",
        "Collect Stand 1",
        "Collect Stand 2",
        "Collect Stand 3",
        "CollectToWound",
        "Crest Shrine Powerup Loop",
        "Dress Flourish",
        "DropToWounded",
        "GetUpToIdle",
        "Give Dress",
        "Give Dress Idle",
        "Hurt Talk Down",
        "Hurt Talk Up",
        "Hurt Talk Up Windy",
        "Idle Hurt Listen",
        "Idle Hurt Listen Backward",
        "Idle Hurt Listen Windy",
        "Idle Hurt Talk",
        "Idle Hurt Talk Backward",
        "Idle Hurt Talk Turn Backward",
        "Idle Hurt Talk Turn Forward",
        "Idle Hurt Talk Windy",
        "Kneel To Prostrate",
        "Kneeling",
        "Look Down Talk",
        "Look Up Half Talk",
        "Look Up Talk",
        "Prostrate",
        "Prostrate NoNeedle",
        "Prostrate Rise",
        "Prostrate Rise NoNeedle",
        "Prostrate Rise Slow",
        "ProstrateRiseToKneel",
        "ProstrateRiseToKneel NoLoop",
        "ProstrateRiseToWound",
        "Respawn Wake",
        "Roar Lock",
        "Roar To LookUp",
        "Talking Backward",
        "Talking Standard",
        "ToChallengeTalk",
        "TurnToChallengeIdle",
        "TurnToChallengeStrong",
        "TurnToChallengeTalk",
        "Wake",
        "Wake To Sit",
        "Wake Up Ground",
        "Weaver Pray",
        "Weaver Pray End",
        "Weaver Pray Prepare",
        "Weaver Pray Prepare Front",
    ];

    protected override bool Callback()
    {
        var hc = HeroController.instance;
        if (hc == null) return false;

        var clip = hc.gameObject.GetComponent<tk2dSpriteAnimator>().CurrentClip?.name;
        return clip != null && CLIPS.Contains(clip);
    }
}
