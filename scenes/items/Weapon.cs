using Godot;
using MyFirst3DGame.scenes.characters.states;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.Items;

public partial class Weapon : PickableItem
{
	[Export] public string ConfigFilePath { get; set; } = "res://configs/weapons.json";
	[Export] public string EnemyGroupName { get; set; } = "enemy";
	[Export] public int RaycastAmount { get; set; } = 7;

	/// <summary>
	/// <![CDATA[What weapon slot the weapon takes up.
	/// 	1. Primary
	/// 	2. Secondary]]>
	/// </summary>
	[Export] public int WeaponSlot { get; set; } = 1;
	public string WeaponType { get; set; }
	public WeaponInventory ParentWeaponInventory { get; set; }

	private double _baseDamage = 25;
	private double _mass = 2.5;

	private readonly char[] _trailingChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

	public Dictionary<string, string> Moves = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Getting config info from JSON file
		if (!FileAccess.FileExists(ConfigFilePath))
		{
			GD.PrintErr($"Could not find weapon config file. {ConfigFilePath}");
			return;
		}

		using var file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Read);

		string jsonString = file.GetAsText();
		file.Close();

		var json = new Json();
		var error = json.Parse(jsonString);

		string nameString = Name.ToString();
		string weaponName = (char.ToLower(nameString[0]) + nameString[1..]).TrimEnd(_trailingChars);
		GD.Print(weaponName);

		if (error == Error.Ok)
		{
			var jsonData = (Godot.Collections.Dictionary)json.Data;
			var weapons = (Godot.Collections.Dictionary)jsonData["weapons"];
			var weaponData = (Godot.Collections.Dictionary)weapons[weaponName];

			_baseDamage = (double)weaponData["damage"];
	 		_mass = (double)weaponData["mass"];
			WeaponType = (bool)weaponData["oneHanded"]? "one_handed" : "two_handed";

			GD.Print($"{weaponName} - Base Damage: {_baseDamage} | Weight: {_mass} kg | Type {WeaponType}");

			Moves.Add("unsheathe1", "unsheathe_" + WeaponType);
			Moves.Add("attack1", "slash1_" + WeaponType);
			Moves.Add("attack2", "stab_" + WeaponType);
			Moves.Add("attack3", "slash3_" + WeaponType);
		}
		else
		{
			GD.PushWarning("Could not find config file");
		}
	}

    public override void PickedUp(Humanoid humanoid)
    {
		IsPickedUp = true;
        WeaponInventory weaponInventory = humanoid.WeaponInventory;
		ParentWeaponInventory = humanoid.WeaponInventory;
		PhysicalBody.AddCollisionExceptionWith(humanoid.Character);
		PhysicalBody.CollisionMask = 2;
		weaponInventory.AddWeaponToInventory(this);
    }
}