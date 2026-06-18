using Godot;
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;
using System.Text;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class Animator : Node
{
    [Export] public HumanoidModel Humanoid { get; set; }
    [Export] public Skeleton3D Skeleton { get; set; }

    [Export] public AnimationPlayer BodyAnimator { get; set; }
    [Export] public AnimationPlayer LegsAnimator { get; set; }

    private string _currentBodyAnimation;
    private string _currentLegsAnimation;

    public int LegsAnimationSpeed { get; set; } = 1;
    public int BodyAnimationSpeed { get; set; } = 1;

    public float SpeedScale { get { return BodyAnimator.SpeedScale; } }

    private bool _animateFullBody = true;
    private float _syncDelta = .01f;
    
    private readonly StringBuilder _stringBuilder = new();

    private readonly Dictionary<string, float> _animationBlendTimes = new()
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

    private void SetBodyAnimation(string animation)
    {
        if (animation.Equals(_currentBodyAnimation)) return;
        
        _currentBodyAnimation = animation;
        BodyAnimator.Play(
            name: _stringBuilder.Append("human_body_animation_library/").Append(animation).ToString(),
            customBlend: _animationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
            customSpeed: BodyAnimationSpeed,
            fromEnd: BodyAnimationSpeed < 0
        );

        _stringBuilder.Clear();
    }

    private void SetLegsAnimation(string animation)
    {
        LegsAnimator.Play(
            name: _stringBuilder.Append("human_legs_animation_library/").Append(animation).ToString(),
            customBlend: _animationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
            customSpeed: LegsAnimationSpeed,
            fromEnd: LegsAnimationSpeed < 0
        );

        _stringBuilder.Clear();
    }

    private void SetAnimations()
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

    private void SyncAnimations()
    {
        double bodyPos = BodyAnimator.CurrentAnimationPosition;
        double legsPos = LegsAnimator.CurrentAnimationPosition;
        double difference = Mathf.Abs(bodyPos - legsPos);

        if (difference > _syncDelta)
        {
            BodyAnimator.Seek(legsPos);
        }
    }

    private void UpdateSyncMode() => _animateFullBody = Humanoid.CurrentState.AnimateFullBody;

    public void SetSpeedScale(float speed)
    {
        LegsAnimator.SpeedScale = speed;
        BodyAnimator.SpeedScale = speed;
    }

    public void ResetSpeedScale()
    {
        LegsAnimator.SpeedScale = 1;
        BodyAnimator.SpeedScale = 1;
    }

    public void SetBodySpeedScale(float speed) => BodyAnimator.SpeedScale = speed;
    public void SetLegsSpeedScale(float speed) => LegsAnimator.SpeedScale = speed;
    public float GetBodySpeedScale() => BodyAnimator.SpeedScale;
    public float GetLegsSpeedScale() => LegsAnimator.SpeedScale;

    private void MoveBodyAnimationToEnd() => BodyAnimator.Advance(BodyAnimator.CurrentAnimation.ToString().Length);
    private void MoveLegsAnimationToEnd() => LegsAnimator.Advance(LegsAnimator.CurrentAnimation.ToString().Length);

    private void ResetBodyAnimation() => BodyAnimator.Seek(0);
    private void ResetLegsAnimation() => LegsAnimator.Seek(0);

    public string GetAnimationWeaponModifier() => Humanoid.CurrentWeapon is null ? "" : "_" + Humanoid.CurrentWeapon.WeaponType;
    
    public static string GetAnimationDirectionModifier(Vector2 direction)
    {
        return direction switch
        {
            {X: > 0, Y: >= 0} or {X: < 0, Y: < 0} => "_left",
            {X: < 0, Y: >= 0} or {X: > 0, Y: < 0} => "_right",
            _ => direction.Y switch
            {
                > 0 when direction.X == 0 => "_front",
                < 0 when direction.X == 0 => "_back",
                _ => ""
            }
        };
    }

    public void QuickEndAttackAnimation()
    {
        float animLen = (float)BodyAnimator.CurrentAnimationLength;
        float animPos = (float)BodyAnimator.CurrentAnimationPosition;

        if (animPos > animLen / 2) return;

        BodyAnimator.Advance(animLen - animPos);
    }
}
