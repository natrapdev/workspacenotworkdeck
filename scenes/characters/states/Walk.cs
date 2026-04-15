using Godot;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Walk : State
{
	private float _walkspeed = 1.5f;
	private float _animSpeed = 1f;
	[Export] private float AccelerationSpeed { get; set; } = .6f;

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
		string animationDirection = Animator.GetAnimationDirectionModifier(input.Direction);
		string animationWeapon = Animator.GetAnimationWeaponModifier();
		string animation = animationDirection.Contains("left") || animationDirection.Contains("right") ? "strafe" : "walk";

		Animation = animation + animationDirection + animationWeapon;

		Vector3 velocity = Character.Velocity;
		Vector3 direction = (Humanoid.Transform.Basis * new Vector3(input.Direction.X, 0, input.Direction.Y)).Normalized();

		float stamina = Resource.CurrentStamina;
		float targetSpeed = (float)(
			stamina >= 0.4 ? _walkspeed : _walkspeed - (70 * Mathf.Pow(stamina - 0.45, 4))
		);

		velocity.X = Mathf.MoveToward(Character.Velocity.X, direction.X * targetSpeed, AccelerationSpeed);
		velocity.Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * targetSpeed, AccelerationSpeed);

		_animSpeed = input.Direction.Y < 0 ? -1 : 1;
		var speedModifier = velocity.Length() / _walkspeed;
		Animator.SetSpeedScale(_animSpeed * speedModifier);

		Character.Velocity = velocity;
	}

	public override void OnEnter()
	{
		Animator.SetSpeedScale(_animSpeed);
	}

	public override void OnExit()
	{
		Animator.ResetSpeedScale();
	}
}