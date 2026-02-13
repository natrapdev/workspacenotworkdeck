using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public interface IState
{
    [Export] string StateName { get; set; }
    [Export] string Animation { get; set; }
    [Export] string BackendAnimation { get; set; }
    [Export] bool AnimateFullBody { get; set; }
    [Export] bool CanBeLongerThanAnimation { get; set; }


    HumanoidModel Humanoid { get; set; }
    HumanoidResource Resource { get; set; }
    Animator Animator { get; set; }
    Skeleton3D Skeleton { get; set; }
    HumanoidCombat Combat { get; set; }
    StateData StateData { get; set; }
    HumanoidStates Parent { get; set; }
    State NextState { get; set; }

    State ChangeState(InputPackage input);
    void Update(InputPackage input, float delta);
    void Enter();
    void OnEnter();
    void Exit();
    void OnExit();
    void TrackLookDirection(InputPackage input, float delta);
    void ForceState(State state);
    State FindFirstValidState(InputPackage input);
    void UpdateResource(float delta);
    State DefaultLifecycle(InputPackage input);
    void CheckFollowUps(InputPackage input);

    bool ExceedsTimeLength(float time);
    bool ElapsedTimeIsBetween(float start, float end);
    void StartTimer();
    void StopTimer();

    bool TransitionsToQueued();
    bool AcceptsQueueing();
    bool TracksLookDirection();
    bool IsVulnerable();
    bool IsInterruptable();
    bool IsParryable();
    bool RightWeaponHurts();
}