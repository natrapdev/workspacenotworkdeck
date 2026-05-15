using Godot;
using System.Collections.Generic;
using System.Text;

namespace Viewport;

public partial class ViewportAnimator : Node
{
    [Export] public ViewportModel Viewport { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    
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

    private string _currentAnimation;
    public float PlaySpeed = 1;

    public void SetAnimation(string animation)
    {
        if (!AnimationPlayer.HasAnimation("viewport_animation_library/" + animation))
        {
            if (_currentAnimation is null)
            {
                Viewport.Skeleton.ResetBonePoses();
            }
            
            return;
        }

        if (animation == _currentAnimation) return;
        
        AnimationPlayer.Play(
            name: _stringBuilder.Append("viewport_animation_library/").Append(animation).ToString(),
            customBlend: _animationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
            customSpeed: PlaySpeed,
            fromEnd: PlaySpeed < 0
        );

        _stringBuilder.Clear();

        _currentAnimation = animation;
    }
}