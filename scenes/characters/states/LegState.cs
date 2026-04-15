using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class LegState : Node, ILegState
{
    public HumanoidModel Humanoid { get; set; }
    public HumanoidStates StateContainer { get; set; }
    public HumanoidLegStates Parent { get; set; }
    public State CurrentState { get; set; }

    public virtual async Task Update(InputPackage input, float delta)
    {
        UpdateLegsState(input, delta);
        await CurrentState.Update(input, delta);
    }

    public virtual void UpdateLegsState(InputPackage input, float delta) { }

    public virtual void ChangeState(State state)
    {
        CurrentState.Exit();
        CurrentState = state;
        CurrentState.Enter();
        Parent.CurrentState = CurrentState;
        Humanoid.Animator.UpdateLegsAnimation();
    }
}
