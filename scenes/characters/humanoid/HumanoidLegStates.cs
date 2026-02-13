using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidLegStates : Node
{
    [Export] HumanoidModel Humanoid { get; set; }

    public State CurrentState { get; set; }

    public void AcceptStates()
    {
        foreach (var child in GetChildren())
        {
            if (child is LegState state)
            {
                state.Humanoid = Humanoid;
                state.StateContainer = Humanoid.StateContainer;
                state.Parent = this;
                state.CurrentState = CurrentState;
            }
        }
    }
    
}
