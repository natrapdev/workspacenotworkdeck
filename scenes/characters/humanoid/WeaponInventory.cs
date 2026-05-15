using Godot;
using MyFirst3DGame.Items;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class WeaponInventory : Node
{
    public Weapon PrimaryWeapon { get; private set; }
    public Weapon SecondaryWeapon { get; private set; }
    public Weapon Dagger { get; private set; }

    [Export] public string PrimaryWeaponContainerPath { get; set; } = "rig/Skeleton3D/HipPrimaryAttachment/Container";
    [Export] public string RightHandWeaponContainerPath { get; set; } = "rig/Skeleton3D/RightHandAttachment/Container";
    [Export] public Skeleton3D Skeleton { get; set; }
    [Export] public HumanoidModel Humanoid { get; set; }
    private Weapon _equippedWeapon = null;

    public void AddWeaponToInventory(Node3D item)
    {
        if (item is not Weapon weapon) return;
        
        switch (weapon.WeaponSlot)
        {
            case 1: GrabPrimaryWeapon(weapon); break;
            // case 2: SecondaryWeapon ??= weapon; break;
            default: GD.PushWarning("Invalid weapon type"); break;
        }
    }

    public Weapon GetWeapon(int slot)
    {
        return slot switch
        {
            1 => PrimaryWeapon,
            2 => SecondaryWeapon,
            3 => Dagger,
            _ => null,
        };
    }

    public Weapon GetEquippedWeapon()
    {
        return _equippedWeapon;
    }

    public void EquipWeapon(int slot)
    {
        _equippedWeapon ??= slot switch
        {
            1 => PrimaryWeapon,
            2 => SecondaryWeapon,
            3 => Dagger,
            _ => throw new NullReferenceException()
        };

        MoveWeaponToRightHand(_equippedWeapon);
    }

    public void UnEquipWeapon()
    {
        switch (_equippedWeapon.WeaponSlot)
        {
            case 1: MoveWeaponToPrimarySlot(); break;
            // TODO: other weapon slots
            default: throw new NullReferenceException();
        }

        _equippedWeapon = null;
    }

    private void GrabPrimaryWeapon(Weapon weapon)
    {
        if (PrimaryWeapon is not null) return;
        
        PrimaryWeapon = weapon;
        MoveWeaponToPrimarySlot();
    }

    private void MoveWeaponToPrimarySlot() => MoveWeaponToSlot(PrimaryWeapon, PrimaryWeaponContainerPath);

    private void MoveWeaponToRightHand(Weapon weapon) => MoveWeaponToSlot(weapon, RightHandWeaponContainerPath);

    private void MoveWeaponToSlot(Weapon weapon, string path)
    {
        Node parent = weapon.GetParent();
        parent.RemoveChild(weapon);
        parent = GetItemContainer(path);
        parent.AddChild(weapon);

        weapon.Position = Vector3.Zero;
        weapon.RotationDegrees = Vector3.Zero;
        weapon.ActualMesh.SetLayerMaskValue(2, path.Contains("Viewport"));
    }

    private Node3D GetItemContainer(string path) => Humanoid.GetNode<Node3D>(path);
}