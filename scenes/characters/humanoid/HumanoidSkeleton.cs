using Godot;
using MyFirst3DGame.scenes.characters.states;
using System;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class HumanoidSkeleton : Skeleton3D
{
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public PhysicalBoneSimulator3D PhysicalSkeleton { get; set; }
	[Export] public Ccdik3D SpineIK { get; set; }

	private bool _ragdolled = false;

	public void ToggleRagdoll()
	{
		_ragdolled = !_ragdolled;

		if (_ragdolled)
		{
			Ragdoll();
		}

	}

	public void Ragdoll()
	{
		if (!PhysicalSkeleton.IsSimulatingPhysics())
		{
			SpineIK.Active = false;
			Humanoid.GetNode<CollisionShape3D>("../Collision").Disabled = true;
			Humanoid.Animator.BodyAnimator.Active = false;
			Humanoid.Animator.LegsAnimator.Active = false;
			PhysicalSkeleton.Active = true;
			PhysicalSkeleton.PhysicalBonesStartSimulation();
		}
	}
}
