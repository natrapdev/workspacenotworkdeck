using Godot;
using System;
using MyFirst3DGame.scenes.characters.humanoid;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedThrust : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public Task LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    public override void OnUpdate(InputPackage input, float delta)
    {
        LegsUpdate(input, delta);
    }

    // public override async Task<HitInfo> RightWeaponHitScan()
    // {
    //     HitInfo hitInfo = await Combat.ScanForHitsStab();
    //     return hitInfo;
    // }

    public override async Task<HitInfo> RightWeaponHitScan()
    {
        HitInfo hitInfo = await Combat.ScanForHitsStab();

        if (hitInfo.HitNode is not null)
        {
            if (hitInfo.HitNode.GetParent() is BoneAttachment3D hitBone)
            {
                var parent = hitBone.GetParent();

                if (parent is DamageModel damageModel)
                {
                    damageModel.Hit(hitInfo);
                }
            }
        }
        return hitInfo;
    }

    public override void OnEnter()
    {
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash_prepare_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash_prepare_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("thrust_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("thrust_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("idle")))
        {
            FollowUpStates.Add(Parent.GetStateByName("idle"));
        }
    }

    public override State DefaultLifecycle(InputPackage input)
    {
        if (Mathf.Abs(Animator.BodyAnimator.CurrentAnimationPosition - Duration) < .35)
        {
            return FindFirstValidState(input);
        }

        return this;
    }
}