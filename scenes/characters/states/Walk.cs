using Godot;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Walk : State
{
	[Export] public float WalkSpeed = 1.5f;
	private float _animSpeed = 1f;
	private const float AnimationBaseSpeed = 1.1f;
	[Export] private float AccelerationSpeed { get; set; } = .6f;

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
		string animationDirection = Animator.GetAnimationDirectionModifier(input.Direction);
		string animationWeapon = Animator.GetAnimationWeaponModifier();
		string animation = animationDirection.Contains("left") || animationDirection.Contains("right") ? "strafe" : "walk";

		Animation = StringBuilder.Append(animation).Append(animationDirection).Append(animationWeapon).ToString();

		Vector3 velocity = Character.Velocity;
		Vector3 direction = (Humanoid.Transform.Basis * new Vector3(input.Direction.X, 0, input.Direction.Y)).Normalized();

		float stamina = Resource.CurrentStamina;
		float targetSpeed = (float)(
			stamina >= 0.4 ? WalkSpeed : WalkSpeed - 70 * Mathf.Pow(stamina - 0.45, 4)
		);

		velocity.X = Mathf.MoveToward(Character.Velocity.X, direction.X * targetSpeed, AccelerationSpeed);
		velocity.Z = Mathf.MoveToward(Character.Velocity.Z, direction.Z * targetSpeed, AccelerationSpeed);

		_animSpeed = input.Direction.Y < 0 ? -1 : 1;
		float speedModifier = velocity.Length() / AnimationBaseSpeed;

		if (HumanoidLegs.CurrentState == Humanoid.CurrentState)
		{
			Animator.SetSpeedScale(_animSpeed * speedModifier);
		}
		else
		{
			Animator.SetBodySpeedScale(1);
			Animator.SetLegsSpeedScale(_animSpeed * speedModifier);
		}

		Character.Velocity = velocity;

		StringBuilder.Clear();
	}

	protected override void OnEnter()
	{
		Animator.SetSpeedScale(_animSpeed);
	}

	protected override void OnExit()
	{
		Animator.ResetSpeedScale();
	}
}