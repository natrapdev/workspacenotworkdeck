using Godot;
using System;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class UnsheathePrimary : State
{
	public Vector3 Spine004LookOriginOffset = new(0.542f, 0, 0);

	[Export] public LookAtModifier3D BodyLookAt { get; set; }

	public override State ChangeState(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return Parent.GetStateByName("airborne");
		}

		return DefaultLifecycle(input);
	}

	protected override void OnUpdate(InputPackage input, float delta)
	{
		if (CanMoveHeldItem()
      		&& Humanoid.WeaponInventory.GetEquippedWeapon() is null
      		&& Humanoid.CurrentWeapon is null)
		{
			Humanoid.WeaponInventory.EquipWeapon(1);
		}
		else if (CanMoveHeldItem()
				&& Humanoid.WeaponInventory.GetEquippedWeapon() is not null
				&& Humanoid.CurrentWeapon is not null)
		{
			Humanoid.WeaponInventory.UnEquipWeapon(1);
		}
	}


	protected override void OnEnter()
	{
		Animator.SetSpeedScale(1);
		Character.Velocity = Vector3.Zero;
		// animPlayback = Animator?.PlayUnsheatheAnimation(CharacterHumanoid.WeaponInventory.PrimaryWeapon);
		// await animPlayback;

		// lookModifier.OriginBoneName = "spine.006";
		// GD.Print(HeadLookAt.BoneName);

		if (Humanoid.CurrentWeapon is null)
		{
			Animator.BodyAnimationSpeed = 1;
			Animator.LegsAnimationSpeed = 1;

			// Humanoid.Resource.HeadLookAtModifier.BoneName = "spine.003";
			// Humanoid.Resource.HeadLookAtModifier.OriginOffset = Spine004LookOriginOffset;
		}
		else
		{
			Animator.BodyAnimationSpeed = -1;
			Animator.LegsAnimationSpeed = -1;
			// BodyLookAt.Active = false;

			// Humanoid.Resource.HeadLookAtModifier.BoneName = "spine.006";
			// Humanoid.Resource.HeadLookAtModifier.OriginOffset = Vector3.Zero;
		}
	}

	protected override void OnExit()
	{

		// BodyLookAt.Active = Humanoid.CurrentWeapon is null;

		Animator.BodyAnimationSpeed = 1;
		Animator.LegsAnimationSpeed = 1;
		Humanoid.CurrentWeapon = Humanoid.WeaponInventory.GetEquippedWeapon();
	}
}
