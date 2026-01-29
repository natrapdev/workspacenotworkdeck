using Godot;
using System;
using System.ComponentModel;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class InputGatherer : Node
{
	public Resources CharacterResources { get; set; }
	public Inventory CharacterInventory { get; set; }
	public WeaponInventory CharacterWeaponInventory { get; set; }
	public Humanoid CharacterHumanoid { get; set; }

	public override void _Ready()
	{
		CharacterHumanoid = GetNode<Humanoid>("../Humanoid");
		CharacterResources = CharacterHumanoid.GetNode<Resources>("Resource");
		CharacterInventory = CharacterHumanoid.GetNode<Inventory>("Inventory");
		CharacterWeaponInventory = CharacterInventory.GetNode<WeaponInventory>("WeaponInventory");
	}

	public InputPackage GatherInput()
	{
		InputPackage newInput = new();

		newInput.actions.Add("idle");

		newInput.direction = Input.GetVector("move_right", "move_left", "move_back", "move_forward");

		if (newInput.direction != Vector2.Zero && CharacterHumanoid.CanMove)
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
			if (CharacterResources.ItemFocus is not null)
			{
				newInput.actions.Add("interact");
			}
		}

		if (Input.IsActionJustPressed("unsheathe1") && CharacterWeaponInventory.GetEquippedWeapon() is null && CharacterWeaponInventory.PrimaryWeapon is not null)
		{
			newInput.actions.Add("unsheathe1");
		}

		// if (Input.IsActionJustPressed("unsheathe2") && CharacterResources.EquippedWeapon is null)
		// {
		// 	newInput.actions.Add("unsheathe2");
		// }

		return newInput;
	}
}
