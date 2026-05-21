using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using MyFirst3DGame.scenes.characters.humanoid;
using System.Text;

namespace MyFirst3DGame.scenes.characters.states;

public partial class State : Node
{
    [Export] public int Priority { get; set; }
    [Export] public bool AnimateFullBody { get; set; } = true;
    [Export] public string Animation { get; set; }
    [Export] public string BackendAnimation { get; set; }
    [Export] public string StateName { get; set; }
    [Export] public float BodyRotationSpeed { get; set; }
    [Export] public bool CanBeLongerThanAnimation { get; set; }
    [Export] public float StaminaCost { get; set; }
    [Export] public float FatigueCost { get; set; }

    public CharacterBody3D Character { get; set; }
    public Node3D CharacterModel { get; set; }
    public Animator Animator { get; set; }
    public Skeleton3D Skeleton { get; set; }
    public HumanoidModel Humanoid { get; set; }
    public HumanoidResource Resource { get; set; }
    public HumanoidCombat Combat { get; set; }
    public StateData StateData { get; set; }
    public HumanoidStates Parent { get; set; }
    public State NextState { get; set; }
    public HumanoidLegStates HumanoidLegs { get; set; }

    private float ElapsedTimeMilliseconds { get { return _stopwatch.ElapsedMilliseconds; } }
    protected float ElapsedTimeSeconds { get { return ElapsedTimeMilliseconds / 1000; } }

    public float Duration { get; set; } = 0f;
    private readonly Stopwatch _stopwatch = new();

    private bool _canForceState = false;
    private State _forcedState;

    private bool _canQueue = false;
    private State _queuedState;
    
    protected readonly StringBuilder StringBuilder = new();

    public readonly List<State> FollowUpStates = new(3);
    protected static readonly float Gravity = ProjectSettings.GetSetting(
        "physics/3d/default_gravity").As<float>();

    public virtual State ChangeState(InputPackage input)
    {
        if (AcceptsQueueing())
        {
            CheckFollowUps(input);
        }

        if (_canQueue && TransitionsToQueued())
        {
            ForceState(_queuedState);
            _canQueue = false;
            _queuedState = null;
        }

        if (_canForceState && _forcedState is not null)
        {
            _canForceState = false;
            Humanoid.SwitchTo(_forcedState);
            return _forcedState;
        }

        return DefaultLifecycle(input);
    }

    public void Update(InputPackage input, float delta)
    {
        Animator.UpdateAnimations();
        if (TracksLookDirection()) TrackLookDirection(input, delta);
        if (RightWeaponHurts()) ScanForHitsRightWeapon();
        OnUpdate(input, delta);
    }
    protected virtual void OnUpdate(InputPackage input, float delta) { }

    public virtual void Enter()
    {
        Resource.PayCosts(this);
        StartTimer();

        OnEnter();
    }
    protected virtual void OnEnter() { }

    public virtual void Exit()
    {
        OnExit();
    }
    protected virtual void OnExit() { }

    public virtual void TrackLookDirection(InputPackage input, float delta)
    {
        // Vector3 direction = Resource.HeadBoneGlobalTransform.Basis * new Vector3(-input.Direction.X,0,-input.Direction.Y).Normalized();
        // Vector3 characterForward = Character.Basis.Z;
        // float angle = characterForward.SignedAngleTo(direction, Vector3.Up);
        // Character.RotateY(Mathf.Clamp(angle, BodyRotationSpeed * delta, BodyRotationSpeed * delta));

        (float x, float y, float z) = Humanoid.GlobalRotation;
        float targetY = Humanoid.LookAtReference.GlobalRotation.Y;
        float newAngle = Mathf.LerpAngle(y, targetY, BodyRotationSpeed * delta);

        Humanoid.GlobalRotation = new Vector3(x, newAngle, z);
    }

    protected virtual State FindFirstValidState(InputPackage input)
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

    private void ForceState(State forcedState)
    {
        _forcedState = forcedState;
        _canForceState = true;
    }

    public virtual void UpdateResource(float delta) => Resource.Update(delta);

    protected virtual State DefaultLifecycle(InputPackage input)
    {
        if (ExceedsTimeLength(Duration) && !CanBeLongerThanAnimation)
        {
            return FindFirstValidState(input);
        }

        return this;
    }

    private void CheckFollowUps(InputPackage input)
    {
        State followUp = input.Actions.Peek();

        if (FollowUpStates.Contains(followUp) && (_queuedState is null || followUp.Priority > _queuedState?.Priority))
        {
            _canQueue = true;
            _queuedState = followUp;
        }
        else if (followUp == this && NextState is not null && Resource.HasEnoughStamina(NextState) 
                 || followUp == this && NextState is null && Resource.HasEnoughStamina(this))
        {
            _canQueue = true;
            _queuedState = NextState;
        }
        else if (Humanoid.CurrentState is IChildState childState) // Find the base state
        {
            if (childState.BaseState == followUp
                && ((State)childState).NextState is not null
                && ((State)childState).NextState.Priority >= Humanoid.CurrentState.Priority)
            {
                _canQueue = true;
                _queuedState = ((State)childState).NextState;
            }
        }
    }

    protected virtual HitInfo ScanForHitsRightWeapon()
    {
        HitInfo hitInfo = Combat.ScanForHitsSlash();
        
        if (hitInfo.HitNode is null) return hitInfo;
        
        if (hitInfo.HitNode.GetParent() is Limb hitLimb)
        {
            hitLimb.Hit(hitInfo);
        }
        
        return hitInfo;
    }

    private bool ExceedsTimeLength(float time) => ElapsedTimeSeconds > time;
    private bool ElapsedTimeIsBetween(float start, float end) => ElapsedTimeSeconds >= start && ElapsedTimeSeconds <= end;
    private void StartTimer() => _stopwatch.Restart();
    private void StopTimer() => _stopwatch.Stop();

    private bool TransitionsToQueued() => StateData.GetTransitionsToQueued(BackendAnimation, ElapsedTimeSeconds);
    private bool AcceptsQueueing() => StateData.GetAcceptsQueueing(BackendAnimation, ElapsedTimeSeconds);
    private bool TracksLookDirection() => StateData.GetTracksLookDirection(BackendAnimation, ElapsedTimeSeconds);
    private bool IsVulnerable() => StateData.GetVulnerable(BackendAnimation, ElapsedTimeSeconds);
    private bool CanBeInterrupted() => StateData.GetInterruptable(BackendAnimation, ElapsedTimeSeconds);
    private bool CanBeParried() => StateData.GetParryable(BackendAnimation, ElapsedTimeSeconds);
    public bool RightWeaponHurts() => StateData.GetRightWeaponHurts(BackendAnimation, ElapsedTimeSeconds);
    protected bool CanMoveHeldItem() => StateData.GetCanMoveHeldItem(BackendAnimation, ElapsedTimeSeconds);
    private bool IsMovementLocked() => StateData.GetIsMovementLocked(BackendAnimation, ElapsedTimeSeconds);
}