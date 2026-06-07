using Godot;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.scenes.characters.humanoid;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.Items;

public partial class Weapon : PickableItem
{
	[Export] public string ConfigFilePath { get; set; } = "res://configs/weapons.json";
	[Export] public string EnemyGroupName { get; set; } = "enemy";
	[Export] public int RaycastAmount { get; set; } = 10;
	[Export] public float BladeWidth { get; set; } = 0.043f;
	[Export] public float BladeLength { get; set; } = 0.728f;
	[Export] public Marker3D BladeStartMarker { get; set; }
	[Export] public Marker3D BladeEndMarker { get; set; }

	/// <summary>
	/// <![CDATA[What weapon slot the weapon takes up.
	/// 	1. Primary
	/// 	2. Secondary]]>
	/// </summary>
	[Export] public int WeaponSlot { get; set; } = 1;
	public string WeaponType { get; set; } = "";
	public WeaponInventory ParentWeaponInventory { get; set; }

	private double _baseDamage = 25;
	public float Mass { get; private set; } = 2.5f;

	private readonly char[] _trailingChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

	public readonly Dictionary<string, string> Moves = [];

	/// <summary>
	/// Where the <c>Weapon</c> is on the sharpness spectrum within the range 0-1.
	/// </summary>
	/// <example>
	/// <br />0 = Completely blunt (clubs, batons, fists, maces)
	/// <br />0.5 = Partially sharp (worn swords, spiked blunt weapons)
	/// <br />1 = Completely sharp (swords, knives, axes)
	/// </example>
	public float Sharpness { get; private set; }
	public string Material { get; private set; }
	
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
		Error error = json.Parse(jsonString);

		string nameString = Name.ToString();
		string weaponName = (char.ToLower(nameString[0]) + nameString[1..]).TrimEnd(_trailingChars);
		GD.Print(weaponName);

		if (error == Error.Ok)
		{
			var jsonData = (Godot.Collections.Dictionary)json.Data;
			var weapons = (Godot.Collections.Dictionary)jsonData["weapons"];
			var weaponData = (Godot.Collections.Dictionary)weapons[weaponName];

			_baseDamage = (double)weaponData["damage"];
			Mass = (float)weaponData["mass"];
			Sharpness = (float)weaponData["sharpness"];
			WeaponType = (bool)weaponData["oneHanded"] ? "one_handed" : "two_handed";

			GD.Print($"{weaponName} - Base Damage: {_baseDamage} | Weight: {Mass} kg | Type {WeaponType}");

			Moves.Add("slash_prepare", "slash_prepare_" + WeaponType);
			Moves.Add("unsheathe1", "unsheathe_" + WeaponType);
			Moves.Add("attack1", "slash1_" + WeaponType);
			Moves.Add("attack2", "thrust_" + WeaponType);
			Moves.Add("attack3", "slash3_" + WeaponType);
		}
		else
		{
			GD.PushWarning("Could not find config file");
		}

		Material = "steel";
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsPickedUp)
		{
			HandleOffset = PhysicalBody.Position;
		}
	}

	public override void PickedUp(HumanoidModel humanoid)
	{
		IsPickedUp = true;
		WeaponInventory weaponInventory = humanoid.WeaponInventory;
		ParentWeaponInventory = humanoid.WeaponInventory;
		PhysicalBody.AddCollisionExceptionWith(humanoid.Character);
		PhysicalBody.Visible = false;
		ActualMesh.Visible = true;
		// PhysicalBody.CollisionMask = 2;
		PhysicalBody.Position = Vector3.Zero;
		ActualMesh.Position = Vector3.Zero;
		weaponInventory.AddWeaponToInventory(this);
	}
}