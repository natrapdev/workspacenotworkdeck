using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class State : Node, IState
{
    [Export] public int Priority { get; set; }
    [Export] public bool AnimateFullBody { get; set; } = true;
    [Export] public string Animation { get; set; }
    [Export] public string BackendAnimation { get; set; }
    [Export] public string StateName { get; set; }
    [Export] public float BodyRotationSpeed { get; set; }
    [Export] public bool CanBeLongerThanAnimation { get; set; }

    public CharacterBody3D Character { get; set; }
    public Animator Animator { get; set; }
    public Skeleton3D Skeleton { get; set; }
    public HumanoidModel Humanoid { get; set; }
    public HumanoidResource Resource { get; set; }
    public HumanoidCombat Combat { get; set; }
    public StateData StateData { get; set; }
    public HumanoidStates Parent { get; set; }
    public State NextState { get; set; }
    public HumanoidLegStates HumanoidLegs { get; set; }

    public const float Gravity = -9.8f;

    [Export] public float StaminaCost { get; set; }
    [Export] public float FatigueCost { get; set; }

    public float ElapsedTimeMilliseconds { get { return _stopwatch.ElapsedMilliseconds; } }
    public float ElapsedTimeSeconds { get { return ElapsedTimeMilliseconds / 1000; } }

    public float Duration { get; set; } = 0f;
    private readonly Stopwatch _stopwatch = new();

    private bool _canForceState;
    private State _forcedState;

    private bool _canQueue;
    private State _queuedState;

    public virtual State ChangeState(InputPackage input)
    {
        if (AcceptsQueueing())
        {
            GD.Print("can accept queueing");
            CheckFollowUps(input);
        }

        if (_canQueue && TransitionsToQueued())
        {
            GD.Print("Trying to force " + _forcedState);
            ForceState(_queuedState);
            _canQueue = false;
        }

        if (_canForceState)
        {
            _canForceState = false;
            return _forcedState;
        }

        return DefaultLifecycle(input);
    }

    public virtual void Update(InputPackage input, float delta)
    {
        if (TracksLookDirection())
        {
            TrackLookDirection(input, delta);
        }

        Animator.UpdateAnimations();
        OnUpdate(input, delta);
    }
    public virtual void OnUpdate(InputPackage input, float delta) { }

    public virtual void Enter()
    {
        Resource.PayCosts(this);
        StartTimer();

        OnEnter();
    }
    public virtual void OnEnter() { }

    public virtual void Exit()
    {
        OnExit();
    }
    public virtual void OnExit() { }

    public virtual void TrackLookDirection(InputPackage input, float delta)
    {
        // Vector3 direction = Resource.HeadBoneGlobalTransform.Basis * new Vector3(-input.Direction.X,0,-input.Direction.Y).Normalized();
        // Vector3 characterForward = Character.Basis.Z;
        // float angle = characterForward.SignedAngleTo(direction, Vector3.Up);
        // Character.RotateY(Mathf.Clamp(angle, BodyRotationSpeed * delta, BodyRotationSpeed * delta));

        Vector3 characterRotation = Character.GlobalRotation;
        float targetAngle = Character.GetNode<Node3D>("CameraPivot").GlobalRotation.Y;
        float currentAngle = characterRotation.Y;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, BodyRotationSpeed * delta);

        Character.GlobalRotation = new Vector3(characterRotation.X, newAngle, characterRotation.Z);
    }

    public virtual State FindFirstValidState(InputPackage input)
    {
        while (input.Actions.Count > 0)
        {
            State state = input.Actions.Dequeue();

            if (Resource.HasEnoughStamina(state))
            {
                return state;
            }
        }

        GD.PushError("Could not find a valid default/idle state.");

        return null;
    }

    public void ForceState(State forcedState)
    {
        if (forcedState.Priority >= _forcedState.Priority)
        {
            GD.Print("FORCING: " + _forcedState.StateName);
            _forcedState = forcedState;
        }
    }

    public virtual void UpdateResource(float delta) => Resource.Update(delta);

    public State DefaultLifecycle(InputPackage input)
    {
        if (ExceedsTimeLength(Duration) && !CanBeLongerThanAnimation)
        {
            return FindFirstValidState(input);
        }
        return this;
    }

    public void CheckFollowUps(InputPackage input)
    {
        State followUp = input.Actions.Dequeue();
        input.Actions.Enqueue(followUp, followUp.Priority);

        if (followUp == this && NextState is not null && Resource.HasEnoughStamina(this))
        {
            _canQueue = true;
            _queuedState = NextState;
        }

        if (Humanoid.CurrentState is IChildState childState) // Find the base state
        {
            if (childState.BaseState == followUp)
            {
                _canQueue = true;
                _queuedState = (childState as State).NextState;
            }
        }
    }

    public bool ExceedsTimeLength(float time) => ElapsedTimeSeconds > time;
    public bool ElapsedTimeIsBetween(float start, float end) => ElapsedTimeSeconds >= start && ElapsedTimeSeconds <= end;
    public void StartTimer() => _stopwatch.Restart();
    public void StopTimer() => _stopwatch.Stop();

    public bool TransitionsToQueued() => StateData.GetTransitionsToQueued(BackendAnimation, ElapsedTimeSeconds);
    public bool AcceptsQueueing() => StateData.GetAcceptsQueueing(BackendAnimation, ElapsedTimeSeconds);
    public bool TracksLookDirection() => StateData.GetTracksLookDirection(BackendAnimation, ElapsedTimeSeconds);
    public bool IsVulnerable() => StateData.GetVulnerable(BackendAnimation, ElapsedTimeSeconds);
    public bool IsInterruptable() => StateData.GetInterruptable(BackendAnimation, ElapsedTimeSeconds);
    public bool IsParryable() => StateData.GetParryable(BackendAnimation, ElapsedTimeSeconds);
    public bool RightWeaponHurts() => StateData.GetRightWeaponHurts(BackendAnimation, ElapsedTimeSeconds);
    public bool CanMoveHeldItem() => StateData.GetCanMoveHeldItem(BackendAnimation, ElapsedTimeSeconds);
    public bool IsMovementLocked() => StateData.GetIsMovementLocked(BackendAnimation, ElapsedTimeSeconds);
}