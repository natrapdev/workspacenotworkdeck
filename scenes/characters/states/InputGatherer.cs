using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class InputGatherer : Node
{
	public InputPackage GatherInput()
	{
		InputPackage newInput = new();

		newInput.actions.Add("idle");

		newInput.direction = Input.GetVector("move_right", "move_left", "move_back", "move_forward");

		if (newInput.direction != Vector2.Zero)
        {
            newInput.actions.Add("walk");
        }

		if (Input.IsActionJustPressed("jump"))
		{
			if (newInput.actions.Contains("walk"))
			{
				newInput.actions.Add("jump");
			}
		}

		if (Input.IsActionJustPressed("interact"))
		{
			newInput.actions.Add("interact");
		}

		return newInput;
	}
}
