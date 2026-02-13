using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class WalkableState : LegState
{
    public override void UpdateLegsState(InputPackage input, float delta)
    {
        string targetState;

        if (input.Direction != Vector2.Zero)
        {
            targetState = "walk";
        }
        else
        {
            targetState = "idle";
        }

        if (!targetState.Equals(Humanoid.CurrentState.StateName))
        {
            ChangeState(StateContainer.GetStateByName(targetState));
        }
    }
}