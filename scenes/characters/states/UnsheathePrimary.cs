using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class UnsheathePrimary : CharacterState
{
	public override string CheckRelevance(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return "airborne";
		}
		return FindFirstValidState(input);
	}

	public override void OnEnterState()
	{
		CharacterAnimator?.PlayUnsheatheAnimation(CharacterHumanoid.WeaponInventory.PrimaryWeapon);	
	}

}
