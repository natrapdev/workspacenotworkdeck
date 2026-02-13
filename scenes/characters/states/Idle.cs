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

	public override State ChangeState(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return Parent.GetStateByName("airborne");
		}
		return FindFirstValidState(input);
	}

	public override void OnUpdate(InputPackage input, float delta) => Animation = "idle" + Animator.GetAnimationWeaponModifier();

	public override void TrackLookDirection(InputPackage input, float delta)
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

	public override void OnEnter() => Character.Velocity = Vector3.Zero;
}