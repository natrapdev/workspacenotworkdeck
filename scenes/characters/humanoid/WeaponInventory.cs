using Godot;
using MyFirst3DGame.Items;
using System;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class WeaponInventory : Node
{
    public Weapon PrimaryWeapon { get; private set; }
    public Weapon SecondaryWeapon { get; private set; }
    public Weapon Dagger { get; private set; }

    [Export] public string PrimaryWeaponContainerPath { get; set; } = "rig/Skeleton3D/HipPrimaryAttachment/Container";
    [Export] public string SecondaryWeaponContainerPath { get; set; } = "rig/Skeleton3D/HipSecondaryAttachment/Container";
    [Export] public string RightHandWeaponContainerPath { get; set; } = "rig/Skeleton3D/RightHandAttachment/Container";
    [Export] public string LeftHandWeaponContainerPath { get; set; } = "rig/Skeleton3D/LeftHandAttachment/Container";
    [Export] public Skeleton3D Skeleton { get; set; }
    [Export] public HumanoidModel Humanoid { get; set; }
    private Weapon _equippedWeapon = null;
    private Weapon _equippedSecondaryWeapon;

    public void AddWeaponToInventory(Node3D item)
    {
        if (item is not Weapon weapon) return;
        
        switch (weapon.WeaponSlot)
        {
            case 1: GrabPrimaryWeapon(weapon); break;
            case 2: GrabSecondaryWeapon(weapon); break;
            default: GD.PushWarning("Invalid weapon type"); break;
        }
    }

    public void AddWeaponToInventoryAndEquip(Weapon weapon)
    {
        switch (weapon.WeaponSlot)
        {
            case 1: GrabPrimaryWeapon(weapon); break;
            case 2: GrabSecondaryWeapon(weapon); break;
            default: GD.PushWarning("Invalid weapon type"); break;
        }
        
        EquipWeapon(weapon.WeaponSlot);
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
        switch (slot)
        {
            case 1:
            {
                _equippedWeapon = PrimaryWeapon;
                MoveWeaponToRightHand(_equippedWeapon);
                break;
            }
            case 2:
            {
                _equippedSecondaryWeapon = SecondaryWeapon;
                MoveWeaponToLeftHand(_equippedSecondaryWeapon);
                Humanoid.CurrentSecondaryWeapon = _equippedSecondaryWeapon;
                break;
            }
            case 3:
            {
                _equippedWeapon = Dagger;
                break;
            }
            default: throw new NullReferenceException();
        }
    }

    public void UnEquipWeapon(int slot)
    {
        switch (slot)
        {
            case 1:
            {
                MoveWeaponToPrimarySlot();
                _equippedWeapon = null;
                break;
            }
            case 2:
            {
                MoveWeaponToSecondarySlot();
                _equippedSecondaryWeapon = null;
                break;
            }
        }
    }

    private void GrabPrimaryWeapon(Weapon weapon)
    {
        if (PrimaryWeapon is not null) return;
        
        PrimaryWeapon = weapon;
        MoveWeaponToPrimarySlot();
    }

    private void GrabSecondaryWeapon(Weapon weapon)
    {
        if (SecondaryWeapon is not null) return;

        SecondaryWeapon = weapon;
        MoveWeaponToSecondarySlot();
    }

    private void MoveWeaponToPrimarySlot() => MoveWeaponToSlot(PrimaryWeapon, PrimaryWeaponContainerPath);
    private void MoveWeaponToSecondarySlot() => MoveWeaponToSlot(SecondaryWeapon, SecondaryWeaponContainerPath);

    private void MoveWeaponToRightHand(Weapon weapon) => MoveWeaponToSlot(weapon, RightHandWeaponContainerPath);
    private void MoveWeaponToLeftHand(Weapon weapon) => MoveWeaponToSlot(weapon, LeftHandWeaponContainerPath);
    
    private void MoveWeaponToSlot(Weapon weapon, string path)
    {
        var parent = weapon.GetParentOrNull<Node>();
        if (parent is not null) parent.RemoveChild(weapon);
        parent = GetItemContainer(path);
        parent.AddChild(weapon);

        weapon.Position = weapon.HandleOffsetPosition;
        weapon.RotationDegrees = weapon.HandleOffsetRotation;
        weapon.ActualMesh?.SetLayerMaskValue(2, path.Contains("Viewport"));
    }

    private Node3D GetItemContainer(string path) => Humanoid.GetNode<Node3D>(path);
}