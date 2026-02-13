using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class LegState : Node, ILegState
{
    public HumanoidModel Humanoid { get; set; }
    public HumanoidStates StateContainer { get; set; }
    public HumanoidLegStates Parent { get; set; }
    public State CurrentState { get; set; }

    public virtual void Update(InputPackage input, float delta)
    {
        UpdateLegsState(input, delta);
        CurrentState.Update(input, delta);
    }

    public virtual void UpdateLegsState(InputPackage input, float delta) { }

    public virtual void ChangeState(State state)
    {
        CurrentState = state;
        Parent.CurrentState = CurrentState;
        Humanoid.Animator.UpdateLegsAnimation();
    }

}
