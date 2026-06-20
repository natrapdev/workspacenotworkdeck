using Godot;
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

    protected override State DefaultLifecycle(InputPackage input)
    {
        if (Mathf.Abs(Animator.BodyAnimator.CurrentAnimationPosition - Duration) < .2)
        {
            return FindFirstValidState(input);
        }
    
        return this;
    }

    // protected override HitInfo ScanForHit() => Humanoid.Combat.ScanForHitsStab();
}
