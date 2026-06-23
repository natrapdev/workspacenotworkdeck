using Godot;
using MyFirst3DGame.Items;
using System.Diagnostics;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Punch : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public override void _Ready() => FollowUpStates.Add(this);

    protected override void OnEnter()
    {
        Animator.BodyAnimator.Seek(0);
    }

    protected override HitInfo ScanForHit(Weapon weapon) => Combat.ScanForHitsStab(weapon);

    protected override State DefaultLifecycle(InputPackage input)
    {
        if (Mathf.Abs(Animator.BodyAnimator.CurrentAnimationPosition - Duration) < .2)
        {
            return FindFirstValidState(input);
        }

        return this;
    }
}
