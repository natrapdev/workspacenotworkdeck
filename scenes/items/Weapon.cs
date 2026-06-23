using Godot;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.scenes.characters.humanoid;
using System;
using System.Collections.Frozen;
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
	
	public FrozenDictionary<string, string> Moves;

	/// <summary>
	/// Where the <c>Weapon</c> is on the sharpness spectrum within the range 0-1.
	/// </summary>
	/// <example>
	/// <br />0 = Completely blunt (clubs, batons, fists, maces)
	/// <br />0.5 = Partially sharp (worn swords, spiked blunt weapons)
	/// <br />1 = Completely sharp (swords, knives, axes)
	/// </example>
	public float Sharpness { get; private set; }
	[Export] public string Material { get; set; } = "steel";
	
	#nullable enable
	public override void _Ready()
	{
		// Getting config info from JSON file
		if (!FileAccess.FileExists(ConfigFilePath))
		{
			GD.PrintErr($"Could not find weapon config file. {ConfigFilePath}");
			return;
		}

		using FileAccess file = FileAccess.Open(ConfigFilePath, FileAccess.ModeFlags.Read);

		string jsonString = file.GetAsText();
		file.Close();

		var json = new Json();
		Error error = json.Parse(jsonString);

		string weaponName = FormatWeaponName(Name.ToString());

		if (error == Error.Ok)
		{
			var jsonData = (Godot.Collections.Dictionary)json.Data;
			var weapons = (Godot.Collections.Dictionary)jsonData["weapons"];
			var weaponData = (Godot.Collections.Dictionary)weapons[weaponName];

			_baseDamage = (double)weaponData["damage"];
			Mass = (float)weaponData["mass"];
			Sharpness = (float)weaponData["sharpness"];
			WeaponType = (string)weaponData["type"];

			if (!WeaponType.Equals("fist")) AddWeaponMoveSet();
		}
		else
		{
			GD.PushWarning("Could not find config file");
		}
	}
	
	private string FormatWeaponName(string weaponName)
	{
		string result = (
			char.ToLower(weaponName[0]) + weaponName[1..]
			).TrimEnd(_trailingChars);
		return result;
	}

	private void AddWeaponMoveSet()
	{
		Moves = new Dictionary<string, string>()
		{
			{ "slash_prepare", "slash_prepare_" + WeaponType },
			{ "unsheathe1", "unsheathe_" + WeaponType },
			{ "attack1", "slash1_" + WeaponType },
			{ "attack2", "thrust_" + WeaponType },
			{ "attack3", "slash3_" + WeaponType }
		}
		.ToFrozenDictionary();
	}
	
	private string FormatWeaponName(string weaponName)
	{
		string result = (
			char.ToLower(weaponName[0]) + weaponName[1..]
			).TrimEnd(_trailingChars);
		return result;
	}

	private void AddWeaponMoveSet()
	{
		Moves = new Dictionary<string, string>()
		{
			{ "slash_prepare", "slash_prepare_" + WeaponType },
			{ "unsheathe1", "unsheathe_" + WeaponType },
			{ "attack1", "slash1_" + WeaponType },
			{ "attack2", "thrust_" + WeaponType },
			{ "attack3", "slash3_" + WeaponType }
		}
		.ToFrozenDictionary();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsPickedUp && PhysicalBody is not null) HandleOffsetPosition = PhysicalBody.Position;
	}

	public override void PickedUp(HumanoidModel humanoid)
	{
		IsPickedUp = true;
		WeaponInventory weaponInventory = humanoid.WeaponInventory;
		ParentWeaponInventory = humanoid.WeaponInventory;
		if (PhysicalBody is not null)
		{
			PhysicalBody.AddCollisionExceptionWith(humanoid.Character);
			PhysicalBody.Visible = false;
			PhysicalBody.Position = Vector3.Zero;
		}
		if (ActualMesh is not null)
		{
			ActualMesh.Visible = true;
			// PhysicalBody.CollisionMask = 2;
			ActualMesh.Position = Vector3.Zero;
		}
		weaponInventory.AddWeaponToInventory(this);
	}
}