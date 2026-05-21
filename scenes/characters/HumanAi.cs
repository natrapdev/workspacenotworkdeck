using Godot;
using System;
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;
using MyFirst3DGame.scenes.characters.bot;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class HumanAi : InputGatherer
{
    [Export] public string CurrentTask { get; set; } = "idle";

    protected override void GetInputs()
    {
        Actions.Add("idle");

        if (CurrentTask == "idle")
        {
            return;
        }
        else if (CurrentTask == "patrol")
        {
            Actions.Add("walk");
        }
        else if (CurrentTask == "chase")
        {
            Humanoid.HeadLookAtTarget.GlobalPosition = ((Bot)Humanoid.Character).Player.CameraPivot.GlobalPosition;
            InputDirection = new Vector2(0, -1);
            Actions.Add("walk");
        }
    }
}
