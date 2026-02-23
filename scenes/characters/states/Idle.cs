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

    public override void OnUpdate(InputPackage input, float delta)
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

			Vector3 characterRotation = Character.GlobalRotation;
			float targetAngle = Character.GetNode<Node3D>("CameraPivot").GlobalRotation.Y;
			float currentAngle = characterRotation.Y;
			float angleDifference = Mathf.AngleDifference(currentAngle, targetAngle);

			if (Mathf.Abs(angleDifference) >= Mathf.DegToRad(HeadRotationLimitDegrees))
			{
				float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, BodyRotationSpeed * delta * Mathf.Abs(angleDifference));

				Character.GlobalRotation = new Vector3(characterRotation.X, newAngle, characterRotation.Z);
			}
		}
		else
		{
			base.TrackLookDirection(input, delta);
		}
	}
}