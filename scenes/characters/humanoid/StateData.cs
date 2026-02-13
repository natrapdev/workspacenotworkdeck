using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class StateData : Node
{
    [Export] public StateDatabase StateDatabase { get; set; }

    public Vector3 GetRootDeltaPosition(string anim, float progress, float delta)
    {
        var data = StateDatabase.GetAnimation(anim);
        var track = data.FindTrack("StateDatabase:RootPosition", Animation.TrackType.Value);

        if (data.TrackGetKeyCount(track) == 0)
        {
            return Vector3.Zero;
        }

        Vector3 previousPosition = (Vector3)data.ValueTrackInterpolate(track, progress - delta);
        Vector3 currentPosition = (Vector3)data.ValueTrackInterpolate(track, progress);
        Vector3 deltaPosition = currentPosition - previousPosition;

        return deltaPosition;
    }

    public bool GetTransitionsToQueued(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:TransitionsToQueued", timeCode);
    public bool GetAcceptsQueueing(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:AcceptsQueueing", timeCode);
    public bool GetVulnerable(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:IsVulnerable", timeCode);
    public bool GetInterruptable(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:IsInterruptable", timeCode);
    public bool GetParryable(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:IsParryable", timeCode);
    public float GetDuration(string anim) => StateDatabase.GetAnimation(anim).Length;
    public bool GetRightWeaponHurts(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:RightHandWeaponHurts", timeCode);
    public bool GetTracksLookDirection(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:TracksLookDirection", timeCode);
    public bool GetCanMoveHeldItem(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:CanMoveHeldItem", timeCode);
    public bool GetIsMovementLocked(string anim, float timeCode) => StateDatabase.GetBoolValue(anim, "StateDatabase:IsMovementLocked", timeCode);
}
