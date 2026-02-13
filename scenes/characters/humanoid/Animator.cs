using Godot;
using MyFirst3DGame.Items;
using System;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Animator : Node
{
    // public StateModel StateModel { get; private set; }
    // public AnimationTree AnimationTree { get; private set; }
    // [Export] public string BodyStateMachinePath { get; set; } = "parameters/BodyStateMachine/playback";
    // [Export] public string DefaultLocomotionPath { get; set; } = "parameters/DefaultLocomotion/blend_position";
    // [Export] public string OneHandedLocomotionPath { get; set; } = "parameters/OneHandedLocomotion/blend_position";
    // [Export] public string LocomotionBlendPath { get; set; } = "parameters/LocomotionBlend2/blend_amount";
    // [Export] public string LegsBodyBlendPath { get; set; } = "parameters/LegsBodyBlend2/blend_amount";

    // public Humanoid Humanoid { get; set; }

    // public Node3D CharacterModel;
    // public CharacterBody3D Character;

    // private AnimationNodeBlend2 _locomotionBlend;
    // private AnimationNodeStateMachinePlayback _bodyStateMachinePlayback;
    // private AnimationNodeBlend2 _legsBodyBlend;
    // public string CurrentAnimation { get; set; }
    // string locomotionPath;
    // private bool _isPlayingAnimation = false;

    [Export] public HumanoidModel Humanoid { get; set; }
    [Export] public Skeleton3D Skeleton { get; set; }

    [Export] public AnimationPlayer BodyAnimator { get; set; }
    [Export] public AnimationPlayer LegsAnimator { get; set; }

    public string CurrentBodyAnimation { get; set; }
    public string CurrentLegsAnimation { get; set; }

    public int LegsAnimationSpeed { get; set; } = 1;
    public int BodyAnimationSpeed { get; set; } = 1;

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
        SetLegsAnimation(Humanoid.HumanoidLegs.CurrentState.Animation);
    }

    public void SetBodyAnimation(string animation)
    {
        BodyAnimator.Play(
            name: "human_body_animation_library/" + animation, 
            customBlend: -1, 
            customSpeed: BodyAnimationSpeed, 
            fromEnd: BodyAnimationSpeed < 0
        );
    }

    public void SetLegsAnimation(string animation)
    {
        LegsAnimator.Play(
            name: "human_legs_animation_library/" + animation,
            customBlend: -1,
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
        LegsAnimator.SpeedScale = speed;
        BodyAnimator.SpeedScale = speed;
    }

    public void MoveBodyAnimationToEnd() => BodyAnimator.Seek(BodyAnimator.CurrentAnimation.Length);
    public void MoveLegsAnimationToEnd() => LegsAnimator.Seek(LegsAnimator.CurrentAnimation.Length);

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
