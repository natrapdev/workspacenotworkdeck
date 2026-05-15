using Godot;
using System;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;

public partial class InteractWithItem : State
{
    public override State ChangeState(InputPackage input)
    {
        if (!Character.IsOnFloor())
        {
            return Parent.GetStateByName("airborne");
        }
        return FindFirstValidState(input);
    }

    protected override void OnEnter()
    {
        InteractableItem item = Resource.ItemFocus;

        if (item is Weapon weapon && Humanoid.WeaponInventory.PrimaryWeapon is null)
        {
            GD.Print(Humanoid.GetParent().Name + " is picking up something");
            weapon.PickedUp(Humanoid);
        }
    }
}
