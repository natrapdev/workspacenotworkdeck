using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class CharacterController : CharacterBody3D
{
	[Signal] public delegate void UpdateStaminaEventHandler(float stamina);
	[Export] public Node3D CharacterModel { get; set; }
	[Export] public CollisionShape3D CollisionShape { get; set; }
	[Export] public String LocomotionBlendPath { get; set; }
	[Export] public AnimationTree CharacterAnimationTree { get; set; }
	[Export] public float TransitionSpeed { get; set; } = 10.0f;
	[Export] public Node3D ViewportArms { get; set; }

	[Export] public Marker3D HeadLookAtTargetMarker { get; set; }
	[Export] public LookAtModifier3D HeadLookAtModifier { get; set; }
	[Export] public Node3D CameraPivot { get; set; }

	[Export] public float Speed { get; set; } = 3.2f; // The base speed of the player in metres per second
	[Export] public float FallAcceleration { get; set; } = 9.8f; // Downward acceleration in the air, in metres per second
	[Export] public float JumpImpulse { get; set; } = 2.5f; // Upwards speed of character when jumping

	[Export] public float BodyMass { get; set; } = 75f;

	private Vector2 currentInput = Vector2.Zero;
	private Vector2 currentVelocity = Vector2.Zero;
	private float rayLength = 1000.0f;
	private bool ragdolled = false;
	private Skeleton3D characterSkeleton;
	private CollisionShape3D HeadCollisionShape;

	private bool executeAttack = false;
	private int clickCount = 0;
	private const float MAX_CLICK_INTERVAL = 0.275f; // max gap between cliks in milliseconds

	private readonly Vector3 CAMERA_OFFSET = new(0, 0, 0.33f);

	private float accelerationTime =  0.15f;

	private float stamina = 0;

	public override void _Ready()
	{
		UpdateStamina += StaminaChanged;

		// Getting relevant nodes
		HeadLookAtTargetMarker = GetNode<Marker3D>("HeadLookAtTargetMarker");

		characterSkeleton = GetNode<Skeleton3D>("CharacterModel/Armature/Skeleton3D");
				HeadCollisionShape = characterSkeleton.GetNode<PhysicalBoneSimulator3D>("PhysicalBoneSimulator3D")
				.GetNode<PhysicalBone3D>("Physical Bone Neck_2")
				.GetNode<CollisionShape3D>("CollisionShape3D");
		HeadLookAtModifier = GetNode<LookAtModifier3D>("CharacterModel/Armature/Skeleton3D/HeadLookAtModifier");
		HeadLookAtModifier.TargetNode = HeadLookAtModifier.GetPathTo(HeadLookAtTargetMarker);

		characterSkeleton.GetNode<MeshInstance3D>("Head").Visible = false;
	}

	public override void _Process(double delta)
	{
		Vector2 newDelta = currentInput - currentVelocity;

		if (newDelta.Length() > TransitionSpeed * (float)delta)
		{
			newDelta = newDelta.Normalized() * TransitionSpeed * (float)delta;
		}

		currentVelocity += newDelta;
		CharacterAnimationTree.Set(LocomotionBlendPath, currentVelocity);

		/*	-- FPS camera -- */
		CameraPivot.GetNode<Camera3D>("Camera3D").GlobalPosition =
		HeadCollisionShape.GlobalTransform.Basis * CAMERA_OFFSET + HeadCollisionShape.GlobalTransform.Origin;
	}

	private double airTime = 0.0;
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y -= FallAcceleration * (float)delta;
			airTime += delta;
		}
		else
        {
            currentInput = Input.GetVector("move_right", "move_left", "move_back", "move_forward");

			if (airTime > 0)
            {
                GD.Print($"Air time: {airTime} seconds");
				airTime = 0;

				
            }
        }

		Vector3 direction = (CharacterModel.Transform.Basis * new Vector3(currentInput.X, 0, currentInput.Y)).Normalized();

		var acceleration = (Speed / accelerationTime) * (float)delta;

		// Stamina effects the speed the character will walk at
		float targetSpeed = (float)(stamina >= 0.4 ? Speed : Speed - (70 * Mathf.Pow(stamina - 0.45, 4)));
		
		if (direction != Vector3.Zero && IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(Velocity.X, direction.X * targetSpeed, acceleration);
			velocity.Z = Mathf.MoveToward(Velocity.Z, direction.Z * targetSpeed, acceleration);
		}
		else if (direction == Vector3.Zero) // If player is standing still
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, acceleration);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, acceleration);
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor() && stamina > 0.3)
        {
            velocity.Y += JumpImpulse;
			velocity.X *= JumpImpulse/2;
			velocity.Z *= JumpImpulse/2;
        }
		Velocity = velocity;

		if (!ragdolled)
		{
			CharacterAnimationTree.Active = true;
			CollisionShape.Disabled = false;
			MoveAndSlide();
			LookAtMouse();
		}
		else
		{
			CollisionShape.Disabled = true;
			CharacterAnimationTree.Active = false;
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey k)
		{
			if (k.Pressed && k.Keycode == Key.R)
			{
				ragdolled = !ragdolled;

				var ragdollSkeleton = characterSkeleton.GetNode<PhysicalBoneSimulator3D>("PhysicalBoneSimulator3D");
				ragdollSkeleton.Active = ragdolled;

				if (ragdolled == true)
					ragdollSkeleton.PhysicalBonesStartSimulation();
				else
					ragdollSkeleton.PhysicalBonesStopSimulation();
			}
		}
	}

	public void LookAtMouse()
	{
		if (HeadLookAtModifier == null || HeadLookAtModifier.TargetNode == null || ragdolled)
		{
			return;
		}

		var spaceState = GetWorld3D().DirectSpaceState; // Use global coordinates
		var camPivot = GetNode<Node3D>("CameraPivot");

		Vector3 lookAtPos = Quaternion.FromEuler(camPivot.Rotation) * new Vector3(0, 0, 10);
		//	GD.Print($"Look at position: {lookAtPos}");
		HeadLookAtTargetMarker.GlobalPosition = Transform.Origin + lookAtPos;

		if (Velocity != Vector3.Zero && IsOnFloor())
		{
			//Mathf.LerpAngle()
			CharacterModel.RotationDegrees = CharacterModel.RotationDegrees.Lerp(new Vector3(0, camPivot.RotationDegrees.Y, 0), 0.1f);
		}
	}

    public void StaminaChanged(float newStamina) => stamina = newStamina;
}