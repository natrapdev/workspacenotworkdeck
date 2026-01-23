using Godot;
using System;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;
public partial class InteractWithItem : CharacterState
{
    public override string CheckRelevance(InputPackage input)
	{
		if (!character.IsOnFloor())
		{
			return "airborne";
		}
		return FindFirstValidState(input);
	}
    public override void OnEnterState()
    {
        PickableItem item = characterResource.pickableItemFocus;
        GD.Print("Trying to pick up " + characterResource.pickableItemFocus.Name);
        CharacterHumanoid.Inventory.AddItemToInventory(item);

        if (!item.IsPickedUp)
        {
            GD.Print("invenotry full");
        }
    }

}
