using Godot;
using System;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;
public partial class InteractWithItem : CharacterState
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
        InteractableItem item = CharacterResource.ItemFocus;

        if (item is Weapon weapon)
        {
            weapon.PickedUp(CharacterHumanoid);
        }
    }

}
