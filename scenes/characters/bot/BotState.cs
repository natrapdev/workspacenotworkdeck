using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.characters.bot;

public partial class BotState : Node
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
    public HumanoidBotModel Humanoid { get; set; }
    public HumanoidResource Resource { get; set; }
    public HumanoidCombat Combat { get; set; }
    public StateData StateData { get; set; }
    public HumanoidStates Parent { get; set; }
    public State NextState { get; set; }
    public HumanoidLegStates HumanoidLegs { get; set; }

    public float ElapsedTimeMilliseconds { get { return _stopwatch.ElapsedMilliseconds; } }
    public float ElapsedTimeSeconds { get { return ElapsedTimeMilliseconds / 1000; } }

    public float Duration { get; set; } = 0f;
    private readonly Stopwatch _stopwatch = new();



}