using Godot;
using MyFirst3DGame.Items;
using System;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedThrust : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    protected override HitInfo ScanForHit(Weapon weapon) => Humanoid.Combat.ScanForHitsStab(weapon);

    protected override State DefaultLifecycle(InputPackage input)
    {
        if (Mathf.Abs(Animator.BodyAnimator.CurrentAnimationPosition - Duration) < .2)
        {
            return FindFirstValidState(input);
        }
    
        return this;
    }
}