using Godot;

public partial class Weapon : Node3D
{
	[Signal] public delegate void HitEventHandler(double damage, CharacterBody3D target);
	[Signal] public delegate void AttackEventHandler();

	[Export] public string ConfigFilePath { get; set; } = "res://Configurations/weapons.json";
	[Export] public string EnemyGroupName { get; set; } = "enemy";
	[Export] public int RaycastAmount { get; set; } = 7;

	private double baseDamage;
	private double weight;

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
		string weaponName = char.ToLower(nameString[0]) + nameString[1..];

		if (error == Error.Ok)
        {
			var jsonData = (Godot.Collections.Dictionary)json.Data;
			var weapons = (Godot.Collections.Dictionary)jsonData["weapons"];
			var weaponData = (Godot.Collections.Dictionary)weapons[weaponName];
			
			baseDamage = (double)weaponData["damage"];
			weight = (double)weaponData["weight"];

			GD.Print($"{weaponName} - Base Damage: {baseDamage} | Weight: {weight} kg");
        }
    }
}
