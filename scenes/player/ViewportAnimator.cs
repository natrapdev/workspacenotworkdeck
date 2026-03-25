using Godot;
using System.Collections.Generic;

namespace Viewport;

public partial class ViewportAnimator : Node
{
    [Export] public ViewportModel Viewport { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    private readonly Dictionary<string, float> AnimationBlendTimes = new()
    {
        {"slash_prepare_one_handed", 0.2f},
        {"slash1_one_handed", 0.3f},
        {"slash2_one_handed", 0.3f},
        {"slash3_one_handed", 0.25f},
        {"thrust_one_handed", 0.35f},
        {"unsheathe_one_handed", 0.1f},
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
        if (!AnimationPlayer.HasAnimation(animation))
        {
            if (_currentAnimation is not null)
            {
                SetAnimation(_currentAnimation);
            }
            else
            {
                Viewport.Skeleton.ResetBonePoses();
            }

            return;
        }

        if (animation != _currentAnimation)
        {
            AnimationPlayer.Play(
                   name: animation,
                   customBlend: AnimationBlendTimes.TryGetValue(animation, out float value) ? value : 0.2,
                   customSpeed: PlaySpeed,
                   fromEnd: PlaySpeed < 0
            );

            _currentAnimation = animation;
        }
    }
}