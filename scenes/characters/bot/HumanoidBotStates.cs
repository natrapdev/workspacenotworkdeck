using Godot;
using System.Collections.Generic;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.characters.bot;

public partial class HumanoidBotStates : Node
{
    public CharacterBody3D Character { get; set; }
    [Export] public HumanoidBotModel Humanoid { get; set; }
    [Export] public Skeleton3D Skeleton { get; set; }
    [Export] public Animator Animator { get; set; }
    [Export] public HumanoidCombat Combat { get; set; }
    [Export] public HumanoidResource Resource { get; set; }
    [Export] public StateData StateData { get; set; }
    [Export] public HumanoidLegStates HumanoidLegs { get; set; }

    public Dictionary<string, State> States { get; } = [];

    public override void _Ready() => Character = Humanoid.Character;

    public void AcceptStates()
    {
        foreach (var child in GetChildren())
        {
            if (child is BotState state)
            {
                // States.Add(state.StateName, state);
                SetProperties(state);
            }
        }
    }

    private void SetProperties(BotState state)
    {
        state.Humanoid = Humanoid;
        state.Character = Character;
        state.Animator = Animator;
        state.Skeleton = Skeleton;
        state.Resource = Resource;
        state.Combat = Combat;
        state.StateData = StateData;
        // state.Parent = this;
        state.Duration = StateData.GetDuration(state.BackendAnimation);
        state.NextState = state.GetChildOrNull<State>(0);
        state.HumanoidLegs = HumanoidLegs;
        // state.CharacterModel = Humanoid.CharacterModel;
    }

    // public BotState GetBotStateByName(string name) => States[name];
}
