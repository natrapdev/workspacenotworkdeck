using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidLegStates : Node
{
    [Export] public HumanoidModel Humanoid { get; set; }

    public State CurrentState { get; set; }

    public void AcceptStates()
    {
        foreach (Node child in GetChildren())
        {
            if (child is not LegState state) continue;
            state.Humanoid = Humanoid;
            state.StateContainer = Humanoid.StateContainer;
            state.Parent = this;
            state.CurrentState = CurrentState;
        }
    }
    
}
