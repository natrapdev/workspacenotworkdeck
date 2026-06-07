using Godot;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.characters.states;

public partial class StateDatabase : AnimationPlayer
{
    [Export] public HumanoidModel Humanoid { get; set; }
	[Export] public Vector3 RootPosition { get; set; }
    [Export] public bool TransitionsToQueued { get; set; }
    [Export] public bool AcceptsQueueing { get; set; }
    [Export] public bool IsParryable { get; set; }
    [Export] public bool IsVulnerable { get; set; }
    [Export] public bool IsInterruptable { get; set; }
    [Export] public bool IsGrabable { get; set; }
    [Export] public bool RightHandWeaponHurts { get; set; }
    [Export] public bool LeftHandWeaponHurts { get; set; }
    [Export] public bool TracksLookDirection { get; set; }
    [Export] public bool CanMoveHeldItem { get; set; }
    [Export] public bool IsMovementLocked { get; set; }
    [Export] public int MaxStatePriority { get; set; }
    [Export] public int MinStatePriority { get; set; }

    public bool GetBoolValue(string anim, string trackName, float timeCode)
    {
        var data = GetAnimation(anim);
        var track = data.FindTrack(trackName, Animation.TrackType.Value);

        if (track == -1) GD.PushWarning($"Could not find track {trackName}!");

        if (Humanoid.Animator.BodyAnimationSpeed < 0)
        {
            timeCode = data.Length - timeCode;
        }

        timeCode *= Humanoid.Animator.SpeedScale;

        return track != -1 && (bool)data.ValueTrackInterpolate(track, timeCode);
    }
}
