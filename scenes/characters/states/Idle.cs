using System;
using System.Linq;
using Godot;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Idle : State
{
	/// <summary>
	/// How far the head can rotate relative to the body.
	/// </summary>
	[Export] public float HeadRotationLimitDegrees { get; set; } = 60f;

	[Export] private float AccelerationTime { get; set; } = 0.15f;

	public override State ChangeState(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return Parent.GetStateByName("airborne");
		}
		return FindFirstValidState(input);
	}

	protected override void OnUpdate(InputPackage input, float delta)
	{
		Animation = "idle" + Animator.GetAnimationWeaponModifier();

		Vector3 velocity = Character.Velocity;

		velocity.X = Mathf.MoveToward(Character.Velocity.X, 0, AccelerationTime);
		velocity.Z = Mathf.MoveToward(Character.Velocity.Z, 0, AccelerationTime);

		Character.Velocity = velocity;
	}

	public override void TrackLookDirection(InputPackage input, float delta)
	{
		if (Humanoid.CurrentState is not IPartialBodyState)
		{

			(float x, float currentAngle, float z) = Humanoid.GlobalRotation;
			float targetAngle = Humanoid.LookAtReference.GlobalRotation.Y;
			float angleDifference = Mathf.AngleDifference(currentAngle, targetAngle);

			if (!(Mathf.Abs(angleDifference) >= Mathf.DegToRad(HeadRotationLimitDegrees))) return;
			
			float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, BodyRotationSpeed * delta * Mathf.Abs(angleDifference));

			Humanoid.GlobalRotation = new Vector3(x, newAngle, z);
		}
		else
		{
			base.TrackLookDirection(input, delta);
		}
	}
}