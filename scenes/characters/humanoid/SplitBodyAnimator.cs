using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class SplitBodyAnimator : Node
{
    [Export] public HumanoidModel Humanoid { get; set; }
    [Export] public Skeleton3D Skeleton { get; set; }
    
    [Export] public AnimationPlayer BodyAnimator { get; set; }
    [Export] public AnimationPlayer LegsAnimator { get; set; }

    private bool _animateFullBody = true;
    private float _syncDelta = .01f;

    public void UpdateAnimations()
    {
        UpdateSyncMode();
        SetAnimations();
    }

    public void UpdateLegsAnimation()
    {
        UpdateSyncMode();
        // SetLegsAnimation(Humanoid.Legs.CurrentLegsState.Animation);
    }

    public void SetBodyAnimation(string animation)
    {
        BodyAnimator.Play("human_body_animation_library/" + animation);
    }

    public void SetLegsAnimation(string animation)
    {
        LegsAnimator.Play("human_legs_animation_library/" + animation);
    }

    public void SetAnimations()
    {
        if (_animateFullBody)
        {
            SetLegsAnimation(Humanoid.CurrentState.Animation);
            SetBodyAnimation(Humanoid.CurrentState.Animation);
            SyncAnimations();
        }
        else
        {
            // SetLegsAnimation(Humanoid.Legs.CurrentLegsState.Animation);
            SetBodyAnimation(Humanoid.CurrentState.Animation);
        }
    }

    public void SyncAnimations()
    {
        double bodyPos = BodyAnimator.CurrentAnimationPosition;
        double legsPos = LegsAnimator.CurrentAnimationPosition;
        double difference = Mathf.Abs(bodyPos - legsPos);

        if (difference > _syncDelta)
        {
            BodyAnimator.Seek(legsPos);
        }
    }

    public void UpdateSyncMode() => _animateFullBody = Humanoid.CurrentState is State; // PartialState

    public void SetSpeedScale(float speed)
    {
        LegsAnimator.SpeedScale = speed;
        BodyAnimator.SpeedScale = speed;
    }

    public void ResetBodyAnimation() => BodyAnimator.Seek(0);
    public void ResetLegsAnimation() => LegsAnimator.Seek(0);
}
