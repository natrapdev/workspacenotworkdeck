using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class WalkableState : LegState
{
    public override void UpdateLegsState(InputPackage input, float delta)
    {
        string targetState = input.Direction != Vector2.Zero ? "walk" : "idle";

        if (!targetState.Equals(Humanoid.HumanoidLegs.CurrentState.StateName))
        {
            ChangeState(StateContainer.GetStateByName(targetState));
        }
    }
}