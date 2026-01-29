using Godot;
using System;
using System.Data;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Walk : CharacterState
{
	private float _walkspeed = 1.5f;
	private float _accelerationTime = 0.15f;
	private const float _BodyRotationSpeed = 30f;

	public override string CheckRelevance(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return "airborne";
		}

		return FindFirstValidState(input);
	}

	public override void Update(InputPackage input, float delta)
	{
		Vector3 velocity = Character.Velocity;
		Vector3 direction = (CharacterModel.Transform.Basis * new Vector3(input.direction.X, 0, input.direction.Y)).Normalized();

		float stamina = CharacterResource.CurrentStamina();
		float targetSpeed = (float)(stamina >= 0.4 ? _walkspeed : _walkspeed - (70 * Mathf.Pow(stamina - 0.45, 4)));

		velocity.X = Mathf.MoveToward(Character.Velocity.X, direction.X * targetSpeed, _accelerationTime);
		velocity.Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * targetSpeed, _accelerationTime);

		Vector3 characterRotation = CharacterModel.GlobalRotation;
		float targetAngle = HeadBoneAttachment.GlobalRotation.Y;
		float currentAngle = characterRotation.Y;
		float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, _BodyRotationSpeed * delta);

		CharacterModel.GlobalRotation = new Vector3(characterRotation.X, newAngle, characterRotation.Z);
		Character.Velocity = velocity;
	}
	public override void OnEnterState()
	{

	}
	public override void OnExitState()
	{
		
	}
}