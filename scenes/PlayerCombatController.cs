using System;
using Godot;
using Godot.Collections;

public partial class PlayerCombatController : Node3D
{
	[Signal] public delegate void HitEventHandler(double damage);
    [Export] public Node3D CharacterModel { get; set; }
    [Export] public Node3D ViewportArms { get; set; }
    [Export] public string UpperBodyStatePlaybackPath { get; set; }
    [Export] public string UpperBodyBlendPlaybackPath { get; set; }
    [Export] public Node3D HeldWeapon { get; set; }
    [Export] public AnimationTree animationTree { get; set; }
    [Export] public int MiddleSwingAttackType { get; set; } = 1;
    [Export] public int UpperSwingAttackType { get; set; } = 2;
    [Export] public int ThrustAttackType { get; set; } = 3;
    [Export] public int WeaponRaycastAmount { get; set; } = 7;
    [Export] public float WeaponRaycastLength { get; set; } = -.3f;
    [Export] public float SwingSpeed { get; set; } = 25f; // metres per second
    [Export] public float WeaponBluntForceCoefficient { get; set; } = 1.1f;

    public Skeleton3D Skeleton, ViewportSkeleton;
    public Node3D RightHandContainer, HipContainer, ItemContainer, ViewportRightHandContainer;

    private bool holdingWeapon = false;
    private bool executeAttack = false;
    private float lastClickTime = 0f;
    private int clickCount = 0;
    private const float MAX_CLICK_INTERVAL = 0.275f; // max gap between cliks in milliseconds

    // -= PLACEHOLDER =-
    // TODO: Add weapon config to specify if style is OneHanded or TwoHanded
    public string weaponStyleName = "OneHanded";
    private string armBlendPlaybackPath = "parameters/Blend2/blend_amount";
    private string armStatePlaybackPath = "parameters/ArmStateMachine/playback";
    private string armTimeScalePath = "parameters/ArmsTimeScale/scale";
    public AnimationTree ArmAnimationTree;

    private PhysicsDirectSpaceState3D _spaceState;

    private string _configsPath = "res://configs/";

    private float _weaponMass, _weaponBaseDamage, _weaponSharpness, _weapon;

    public override void _Ready()
    {
        var character = GetNode<Node3D>("../CharacterModel");
        Skeleton = character.GetNode<Skeleton3D>("Armature/Skeleton3D");
        RightHandContainer = Skeleton.GetNode<Node3D>("RightHandAttachment/RightHandContainer");
        HipContainer = Skeleton.GetNode<Node3D>("HipAttachment/HipContainer");

		PickUpWeapon(ViewportArms.GetNode<Node3D>("ItemContainer/ArmingSword"));

        ViewportSkeleton = ViewportArms.GetNode<Skeleton3D>("Armature/Skeleton3D");
        ViewportRightHandContainer = ViewportSkeleton.GetNode<Node3D>("RightHandAttachment/RightHandContainer");
        ItemContainer = ViewportArms.GetNode<Node3D>("ItemContainer");
        ArmAnimationTree = ViewportArms.GetNode<AnimationTree>("AnimationTree");

        Skeleton.GetNode<MeshInstance3D>("LeftUpperArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightUpperArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("LeftLowerArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightLowerArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("LeftHand").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightHand").Visible = false;

        UnEquipWeapon();

        _spaceState = GetWorld3D().DirectSpaceState;


    }

    bool isAttacking = false;

    public override void _Process(double delta)
    {
        var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

        isAttacking = false;

        if (playback.GetCurrentNode().Equals("End") && !holdingWeapon)
        {
            UnEquipWeapon();
        }
        else
        {
            if (playback.IsPlaying() &&
            (playback.GetCurrentNode().Equals(weaponStyleName + "MiddleSwing")
            || playback.GetCurrentNode().Equals(weaponStyleName + "UpperSwing")
            || playback.GetCurrentNode().Equals(weaponStyleName + "Thrust")))
            {
                //GD.Print($"Anim length: {playback.GetCurrentLength()}");
                //GD.Print($"Play pos: {playback.GetCurrentPlayPosition()}");
                var camPivot = GetNode<Node3D>("../CameraPivot");
                CharacterModel.RotationDegrees = CharacterModel.RotationDegrees.Lerp(new Vector3(0, camPivot.RotationDegrees.Y, 0), 0.1f);

                if (playback.GetCurrentPlayPosition() < playback.GetCurrentLength() / 2.25
                && playback.GetCurrentPlayPosition() > playback.GetCurrentLength() * 0.2)
                {
                    isAttacking = true;
                } else
                {
                    isAttacking = false;
                }
            }
        }


        if (!executeAttack)
        {
            lastClickTime += (float)delta;

            if ((lastClickTime >= MAX_CLICK_INTERVAL || clickCount >= 3) && holdingWeapon)
            {
                executeAttack = true;
                Attack(clickCount);
                clickCount = 0;
            }
        }

        if (Input.IsActionJustPressed("attack_main"))
        {
            clickCount++;
            lastClickTime = 0f;
            executeAttack = false;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (isAttacking)
        {
			var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);
            var weaponStart = HeldWeapon.GetNode<Marker3D>("BladeStart");
            var weaponEnd = HeldWeapon.GetNode<Marker3D>("BladeEnd");
            float increment = (weaponEnd.Position.Y - weaponStart.Position.Y) / WeaponRaycastAmount;
            Vector3 rayLength = playback.GetCurrentNode().Equals(weaponStyleName + "Thrust") ? new Vector3(0, -WeaponRaycastLength * 3, 0) : new Vector3(WeaponRaycastLength, 0, 0);

            for (int i = 0; i < WeaponRaycastAmount; i++)
            {
				Vector3 origin = weaponStart.GlobalTransform.Basis * new Vector3(0, increment * i, 0) + weaponStart.GlobalTransform.Origin;
				Vector3 targetPosition = weaponStart.GlobalTransform.Basis * rayLength + weaponStart.GlobalTransform.Origin;

                var queryParams = PhysicsRayQueryParameters3D.Create(origin, targetPosition);
                queryParams.CollideWithAreas = true;
                queryParams.CollideWithBodies = true;
                queryParams.CollisionMask = 2;

				queryParams.Exclude = [GetParent<CollisionObject3D>().GetRid()];

                var result = _spaceState.IntersectRay(queryParams);

                if (result.Count > 0)
                {
                    Node3D collider = (Node3D)result["collider"];

                    if (collider.Name.ToString().Contains("Armour"))
                    {
                        HitArmour(result, origin, playback.GetCurrentNode().Equals(weaponStyleName + "Thrust"));
                    }

					MeshInstance3D hit = new();
					collider.AddChild(hit);
					hit.Mesh = new BoxMesh();
					hit.Scale = new Vector3(.1f, .1f, .1f);
					hit.GlobalPosition = (Vector3)result["position"];
                    
                }
            }
        }
    }

    private void HitArmour(Dictionary rayResult, Vector3 rayOrigin, bool isThrust)
    {
        Node3D collider = (Node3D)rayResult["collider"];
        Vector3 hitNormal = (Vector3)rayResult["normal"];
        Vector3 hitPos = (Vector3)rayResult["position"];
        Vector3 rayDir = (hitPos - rayOrigin).Normalized();

        float dotProduct = rayDir.Dot(hitNormal);
        float impactAngle = 180 - Mathf.RadToDeg(Mathf.Acos(dotProduct));

        Node materials = GetNode<Node>("/root/Materials");
        Dictionary materialData = (Dictionary)materials.Get("material_data");

        string colliderMaterial = (string)collider.GetMeta("MaterialName");
        float colliderThickness = (float)collider.GetMeta("Thickness");
        float colliderDensity = (float)((Dictionary)materialData[colliderMaterial])["density"];
        float colliderEnergyAbsorption = (float)((Dictionary)materialData[colliderMaterial])["absorption"];
        
        if (colliderMaterial == "gambeson") colliderDensity *= colliderThickness;

        float colliderThicknessInLineOfSight = Mathf.Abs(colliderThickness / Mathf.Cos(impactAngle)); 
        
        float weaponThickness = isThrust ? .5f : .175f;
        
        // PLACEHOLDER -- STEEL MATERIAL FOR SWORD -- CHANGE SOON PLEASE (2025-12-15)
        float weaponDensity = (float)((Dictionary)materialData["steel"])["density"];

        float impactDepth = weaponThickness * (weaponDensity / colliderDensity);

        float weaponVelocityAtImpactAngle = Mathf.Abs(SwingSpeed * Mathf.Cos(impactAngle));
        float weaponSharpnessDivisor = Mathf.Pow(_weaponSharpness, WeaponBluntForceCoefficient) + 1;
        float baseImpactEnergy = _weaponMass / 2 * Mathf.Pow(weaponVelocityAtImpactAngle / weaponSharpnessDivisor, 2);
        float effectiveEnergyAbsorption = colliderEnergyAbsorption - (impactDepth / colliderThicknessInLineOfSight);
        
        float inflictedKineticEnergy = baseImpactEnergy * (1 - effectiveEnergyAbsorption);
        
        GD.Print($"Energy transfer through armour: {inflictedKineticEnergy} Joules");
    }

    private static void CalculateImpactDepth()
    {
        
    }




    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("EquipWeapon"))
        {
            holdingWeapon = !holdingWeapon;

            var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

            if (holdingWeapon)
            {
                playback.Travel("Start");
                EquipWeapon();
            }
            else
            {
                playback.Travel("End");
            }
        }
    }


    public void Attack(int attackType)
    {
        var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

        if (attackType == MiddleSwingAttackType)
        {
            playback.Travel(weaponStyleName + "MiddleSwing");
        }
        else if (attackType == UpperSwingAttackType)
        {
            playback.Travel(weaponStyleName + "UpperSwing");
        }
        else if (attackType == ThrustAttackType)
        {
            playback.Travel(weaponStyleName + "Thrust");
        }


    }

    private void PickUpWeapon(Node3D weapon)
    {
        HeldWeapon = weapon;

        string weaponConfigPath = _configsPath + "weapons.json";

        // Getting config info from JSON file
		if (!FileAccess.FileExists(weaponConfigPath))
        {
            GD.PrintErr($"Could not find weapon config file. {weaponConfigPath}");
			return;
        }

		using var file = FileAccess.Open(weaponConfigPath, FileAccess.ModeFlags.Read);

		string jsonString = file.GetAsText();
		file.Close();

		var json = new Json();
		var error = json.Parse(jsonString);

		string nameString = weapon.Name.ToString();
		string weaponName = char.ToLower(nameString[0]) + nameString[1..];

		if (error == Error.Ok)
        {
			var jsonData = (Godot.Collections.Dictionary)json.Data;
			var weapons = (Godot.Collections.Dictionary)jsonData["weapons"];
			var weaponData = (Godot.Collections.Dictionary)weapons[weaponName];
			
			_weaponBaseDamage = (float)weaponData["damage"];
			_weaponMass = (float)weaponData["mass"];
            _weaponSharpness = (float)weaponData["sharpness"];
            weaponStyleName = (bool)weaponData["oneHanded"] ? "TwoHanded" : "OneHanded";
        }
    }

    public void EquipWeapon()
    {
        ItemContainer.GetNode<MeshInstance3D>("ArmingSword/armingSword").Visible = true;
        var parent = ItemContainer.GetParent();
        parent.RemoveChild(ItemContainer);
        ViewportRightHandContainer.AddChild(ItemContainer);
        ItemContainer.Position = Vector3.Zero;
        ItemContainer.RotationDegrees = Vector3.Zero;
    }

    public void UnEquipWeapon()
    {
        //var parent = ItemContainer.GetParent();
        //parent.RemoveChild(ItemContainer);
        ItemContainer.Position = Vector3.Zero;
        ItemContainer.RotationDegrees = Vector3.Zero;
        ItemContainer.GetNode<MeshInstance3D>("ArmingSword/armingSword").Visible = false;
    }
}
