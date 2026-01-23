using Godot;
using System;
using System.ComponentModel;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class InputGatherer : Node
{
	public Resources CharacterResources { get; set; }

	public override void _Ready()
	{
		try
		{
			CharacterResources = GetNode<Resources>("../Humanoid/Resource");
		}
		catch (NullReferenceException)
		{
			GD.PrintErr("You don't have a humanoid node!");
		}
	}

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
			if (CharacterResources.pickableItemFocus is not null && !CharacterResources.pickableItemFocus.IsPickedUp)
			{
				newInput.actions.Add("interact");
			}
		}

		return newInput;
	}
}
