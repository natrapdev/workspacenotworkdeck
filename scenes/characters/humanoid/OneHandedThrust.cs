using Godot;
using System;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedThrust : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    protected override void OnUpdate(InputPackage input, float delta)
    {
        
    }

    protected override HitInfo ScanForHitsRightWeapon()
    {
        return Combat.ScanForHitsStab();
    }


    protected override State DefaultLifecycle(InputPackage input)
    {
        if (Mathf.Abs(Animator.BodyAnimator.CurrentAnimationPosition - Duration) < .35)
        {
            return FindFirstValidState(input);
        }

        return this;
    }
}