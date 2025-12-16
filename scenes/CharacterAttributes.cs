using Godot;
using System;

public partial class CharacterAttributes : Node
{
	[Signal] public delegate void UpdateStaminaEventHandler(float stamina);
	[Signal] public delegate void HitEventHandler(double damage);
	[Export] public CharacterBody3D Character { get; set; }
	[Export] public double Health { get; set; } = 100;
	[Export] public double MaxStamina { get; set; } = 100;
	[Export] public float StaminaRegen { get; set; } = 3f;
	[Export] public float FatigueGain { get; set; } = .1f;
	[Export] public float WalkingCost { get; set; } = .012f;
	[Export] public float JumpCost { get; set; } = 20f;
	[Export] public float SwingCost { get; set; } = .8f;


	private double healthDepletion = 0.0;
	private float staminaDepletion = 0f;

	private double currentStamina;
	private float currentFatigue = 0;
	private bool ragdolled = false;
	private bool jumped = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hit += OnHit;
		Area3D hitbox = GetNode<Area3D>("../Hitbox");

		currentStamina = MaxStamina;
	}

	public override void _PhysicsProcess(double delta)
	{
		staminaDepletion = 0;

		if (Input.IsActionPressed("move_left")
		|| Input.IsActionPressed("move_right")
		|| Input.IsActionPressed("move_forward")
		|| Input.IsActionPressed("move_back"))
		{
			staminaDepletion += WalkingCost * (float)delta;
		}

		if (Input.IsActionJustPressed("attack_main"))
		{

		}

		if (jumped)
		{
			staminaDepletion += JumpCost;
		}

		float staminaGain = (StaminaRegen - currentFatigue) * (float)delta;

		currentStamina = Mathf.Clamp(currentStamina + (staminaGain - staminaDepletion), 0, MaxStamina);
		jumped = false;

		if (Character.IsInGroup("player"))
		{
			Character.EmitSignal(SignalName.UpdateStamina, currentStamina / MaxStamina);
			Character.GetNode("GameInterface").EmitSignal(SignalName.UpdateStamina, currentStamina / MaxStamina);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("jump") && Character.IsOnFloor() && currentStamina / MaxStamina > 0.3)
		{
			jumped = true;
		}
	}


	public void OnHit(double damage)
	{
		Health -= damage;

		GD.Print($"{GetParent().Name} Health: {Health + damage} -> {Health}");

		if (Health <= 0 && !ragdolled)
		{
			ToggleRagdoll();
		}
	}

	private void ToggleRagdoll()
	{
		ragdolled = !ragdolled;
		var ragdollSkeleton = GetNode<PhysicalBoneSimulator3D>("../CharacterModel/Armature/Skeleton3D/PhysicalBoneSimulator3D");

		if (ragdolled)
		{
			ragdollSkeleton.Active = true;
			GetNode<CollisionShape3D>("../CollisionShape3D").Disabled = true;
			GetNode<AnimationTree>("../AnimationTree").Active = false;
			ragdollSkeleton.PhysicalBonesStartSimulation();
		}
		else
		{
			ragdollSkeleton.Active = false;
			GetNode<CollisionShape3D>("../CollisionShape3D").Disabled = false;
			GetNode<AnimationTree>("../AnimationTree").Active = true;
			ragdollSkeleton.PhysicalBonesStopSimulation();
		}
	}
}