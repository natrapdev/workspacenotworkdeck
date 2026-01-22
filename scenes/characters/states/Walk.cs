using Godot;
using System;
using System.Data;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Walk : CharacterState
{
	private float _walkspeed = 1.5f;
	private float _accelerationTime = 0.15f;

	public override string CheckRelevance(InputPackage input)
	{
		if (!character.IsOnFloor())
		{
			return "airborne";
		}

		return FindFirstValidState(input);
	}

	public override void Update(InputPackage input, float delta)
	{
		Vector3 velocity = character.Velocity;
		Vector3 direction = (characterModel.Transform.Basis * new Vector3(input.direction.X, 0, input.direction.Y)).Normalized();

		animationTree.Set(defaultLocomotionPath, input.direction);

		float stamina = characterResource.CurrentStamina();
		float targetSpeed = (float)(stamina >= 0.4 ? _walkspeed : _walkspeed - (70 * Mathf.Pow(stamina - 0.45, 4)));

		velocity.X = Mathf.MoveToward(character.Velocity.X, direction.X * targetSpeed, _accelerationTime);
		velocity.Z = Mathf.MoveToward(character.Velocity.Z, direction.Z * targetSpeed, _accelerationTime);
		
		// note this is player only because it uses camera pivot. change soon to support all characters
		characterModel.RotationDegrees = characterModel.RotationDegrees.Lerp(new Vector3(0, camPivot.RotationDegrees.Y, 0), 0.1f);

		character.Velocity = velocity;
	}
	public override void OnEnterState()
	{

	}
	public override void OnExitState()
	{
		animationTree.Set(defaultLocomotionPath, Vector2.Zero);
	}
}