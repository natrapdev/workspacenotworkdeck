using Godot;
using MyFirst3DGame.scenes.characters.humanoid;
using MyFirst3DGame.Items;
using System;
using System.Linq;
using System.Reflection;

namespace MyFirst3DGame.scenes;

public partial class WeaponInventoryEditor : Node
{
    [ExportGroup("Force Weapons")]
    [Export] public string PathToPrimaryWeaponScene { get; set; }
    [Export] public bool ForceEquipPrimaryOnStart { get; set; }
    [Export] public string PathToSecondaryWeaponScene { get; set; }
    [Export] public bool ForceEquipSecondaryOnStart { get; set; }
    [Export] public string PathToDaggerScene { get; set; }
    [Export] public bool ForceEquipDaggerOnStart { get; set; }

    private HumanoidModel _humanoid;
    private WeaponInventory _weaponInventory;
    private PropertyInfo[] _properties;

    public override async void _Ready()
    {
        await ToSignal(GetParent(), Node.SignalName.Ready);
        await ToSignal(GetParent().GetParent(), Node.SignalName.Ready);
        
        _humanoid = GetParentOrNull<HumanoidModel>();
        _weaponInventory = _humanoid.WeaponInventory;
        
        ApplyWeaponScenePaths();
    }

    private void ApplyWeaponScenePaths()
    {
        _properties = GetType().GetProperties();

        foreach (PropertyInfo property in _properties.Where(p => p.GetValue(this) is string s && !String.IsNullOrEmpty(s)))
            AddWeapon(property.Name, (string)property.GetValue(this));
    }

    private static int GetWeaponSlotFromPropertyName(string propertyName)
    {
        return propertyName switch
        {
            not null when propertyName.Contains("Primary") => 1,
            not null when propertyName.Contains("Secondary") => 2,
            not null when propertyName.Contains("Dagger") => 3,
            _ => 0
        };
    }

    private bool ShouldForceEquipWeapon(string propertyName)
    {
        return propertyName switch
        {
            not null when propertyName.Contains("Primary") => ForceEquipPrimaryOnStart,
            not null when propertyName.Contains("Secondary") => ForceEquipSecondaryOnStart,
            not null when propertyName.Contains("Dagger") => false,
            _ => throw new Exception($"Invalid property name: {propertyName}")
        };
    }

    private void AddWeapon(string weaponPropertyName, string path)
    {
        if (ShouldForceEquipWeapon(weaponPropertyName))
            MoveWeaponToInventoryAndEquip(path);
        else
            MoveWeaponToInventory(path);
    }

    private static Weapon InstantiateWeaponScene(string path)
    {
        return ((PackedScene)GD.Load(path)).Instantiate<Weapon>();
    }

    private void MoveWeaponToInventoryAndEquip(string path)
    {
        Weapon weapon = InstantiateWeaponScene(path);
        weapon.PickedUp(_humanoid);
        _weaponInventory.EquipWeapon(weapon.WeaponSlot);
        // _weaponInventory.AddWeaponToInventoryAndEquip(InstantiateWeaponScene(path));
    }

    private void MoveWeaponToInventory(string path)
    {
        Weapon weapon = InstantiateWeaponScene(path);
        weapon.PickedUp(_humanoid);
        // _weaponInventory.AddWeaponToInventory(InstantiateWeaponScene(path));
    }
}
