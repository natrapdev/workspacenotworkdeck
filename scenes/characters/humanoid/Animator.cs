using Godot;
using MyFirst3DGame.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Animator : Node
{
    [Export] public HumanoidModel Humanoid { get; set; }
    [Export] public Skeleton3D Skeleton { get; set; }

    [Export] public AnimationPlayer BodyAnimator { get; set; }
    [Export] public AnimationPlayer LegsAnimator { get; set; }

    public string CurrentBodyAnimation { get; set; }
    public string CurrentLegsAnimation { get; set; }

    public int LegsAnimationSpeed { get; set; } = 1;
    public int BodyAnimationSpeed { get; set; } = 1;

    public float SpeedScale { get { return BodyAnimator.SpeedScale; } }

    private bool _animateFullBody = true;
    private float _syncDelta = .01f;

    private readonly Dictionary<string, float> AnimationBlendTimes = new()
    {
        {"slash_prepare_one_handed", 0.2f},
        {"slash1_one_handed", 0.3f},
        {"slash2_one_handed", 0.3f},
        {"slash3_one_handed", 0.25f},
        {"thrust_one_handed", 0.35f},
        {"idle", 0.3f},
        {"idle_one_handed", 0.3f},
        {"walk_front", 0.5f},
        {"walk_back", 0.5f},
        {"strafe_left", 0.45f},
        {"strafe_right", 0.45f}
    };

    public void UpdateAnimations()
    {
        UpdateSyncMode();
        SetAnimations();
    }

    public void UpdateLegsAnimation()
    {
        UpdateSyncMode();
        SetLegsAnimation(Humanoid.HumanoidLegs.CurrentState.Animation);
    }

    public void SetBodyAnimation(string animation)
    {
        if (animation != CurrentBodyAnimation)
        {
            CurrentBodyAnimation = animation;
            BodyAnimator.Play(
                name: "human_body_animation_library/" + animation,
                customBlend: AnimationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
                customSpeed: BodyAnimationSpeed,
                fromEnd: BodyAnimationSpeed < 0
            );
        }
    }

    public void SetLegsAnimation(string animation)
    {
        LegsAnimator.Play(
            name: "human_legs_animation_library/" + animation,
            customBlend: AnimationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
            customSpeed: LegsAnimationSpeed,
            fromEnd: LegsAnimationSpeed < 0
        );
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
            SetLegsAnimation(Humanoid.HumanoidLegs.CurrentState.Animation);
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

    public void UpdateSyncMode() => _animateFullBody = Humanoid.CurrentState.AnimateFullBody;

    public void SetSpeedScale(float speed)
    {
        if (_animateFullBody)
        {
            LegsAnimator.SpeedScale = speed;
            BodyAnimator.SpeedScale = speed;
        }
        else
        {
            LegsAnimator.SpeedScale = speed;
        }
    }

    public void ResetSpeedScale()
    {
        LegsAnimator.SpeedScale = 1;
        BodyAnimator.SpeedScale = 1;
    }

    public void SetBodySpeedScale(float speed) => BodyAnimator.SpeedScale = speed;
    public void SetLegsSpeedScale(float speed) => LegsAnimator.SpeedScale = speed;

    public void MoveBodyAnimationToEnd() => BodyAnimator.Seek(BodyAnimator.CurrentAnimation.ToString().Length);
    public void MoveLegsAnimationToEnd() => LegsAnimator.Seek(LegsAnimator.CurrentAnimation.ToString().Length);

    public void ResetBodyAnimation() => BodyAnimator.Seek(0);
    public void ResetLegsAnimation() => LegsAnimator.Seek(0);

    public string GetAnimationWeaponModifier() => Humanoid.CurrentWeapon is null ? "" : "_" + Humanoid.CurrentWeapon.WeaponType;
    public static string GetAnimationDirectionModifier(Vector2 direction)
    {
        string anim = "";

        if (direction.Y > 0 && direction.X == 0)
        {
            anim = "_front";
        }
        else if (direction.Y < 0 && direction.X == 0)
        {
            anim = "_back";
        }

        if ((direction.X > 0 && direction.Y >= 0) || (direction.X < 0 && direction.Y < 0))
        {
            anim = "_left";
        }
        if ((direction.X < 0 && direction.Y >= 0) || (direction.X > 0 && direction.Y < 0))
        {
            anim = "_right";
        }

        return anim;
    }
}
