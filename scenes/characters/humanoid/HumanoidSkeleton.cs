using Godot;
using System.Diagnostics;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class HumanoidSkeleton : Skeleton3D
{
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public PhysicalBoneSimulator3D PhysicalSkeleton { get; set; }
	[Export] public Ccdik3D SpineIk { get; set; }

	private Stopwatch _ragdollStopwatch = new();
	
	public (PhysicalBone3D left, PhysicalBone3D right) UpperArms;
	public (PhysicalBone3D left, PhysicalBone3D right) Forearms; 
	public (PhysicalBone3D left, PhysicalBone3D right) Thighs;
	public (PhysicalBone3D left, PhysicalBone3D right) Shins;

	private bool _ragdolled = false;

	public override void _Ready()
	{
		UpperArms = (PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone upper_arm_L"), PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone upper_arm_R"));
		Forearms = (PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone forearm_L"), PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone forearm_R"));
		Thighs = (PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone thigh_L"), PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone thigh_R"));
		Shins = (PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone shin_L"), PhysicalSkeleton.GetNode<PhysicalBone3D>("Physical Bone shin_R"));
	}

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
		if (PhysicalSkeleton.IsSimulatingPhysics()) return;
		
		SpineIk.Active = false;
		Humanoid.GetNode<CollisionShape3D>("../Collision").Disabled = true;
		Humanoid.Animator.BodyAnimator.Active = false;
		Humanoid.Animator.LegsAnimator.Active = false;
		PhysicalSkeleton.Active = true;
		PhysicalSkeleton.PhysicalBonesStartSimulation();

		_ragdollStopwatch.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!PhysicalSkeleton.IsSimulatingPhysics()) return;
		RagdollMode();
	}

	private void RagdollMode()
	{
		if (_ragdollStopwatch.ElapsedMilliseconds / 1000 <= 10)
		{
			CurlUp();
		}
		else
		{
			Limp();
		}
	}

	private void CurlUp()
	{
		SetUpperArmAngularVelocity(500);
		// SetForearmsAngularVelocity(50);
		// SetThighsAngularVelocity(50);
		// SetShinsAngularVelocity(50);
	}

	private void Limp()
	{
		SetUpperArmAngularVelocity(0);
		SetForearmsAngularVelocity(0);
		SetThighsAngularVelocity(0);
		SetShinsAngularVelocity(0);
	}

	private void SetUpperArmAngularVelocity(float vel)
	{
		vel = Mathf.DegToRad(vel);

		Vector3 velocity = new(vel, 0, 0);
		
		UpperArms.left.AngularVelocity = UpperArms.left.AngularVelocity + velocity;
		UpperArms.right.AngularVelocity = UpperArms.left.AngularVelocity + velocity;
	}
	
	private void SetForearmsAngularVelocity(float vel)
	{
		vel = Mathf.DegToRad(vel);

		Vector3 velocity = new(vel, 0, 0);
		
		Forearms.left.AngularVelocity = velocity;
		Forearms.right.AngularVelocity = velocity;
	}
	
	private void SetThighsAngularVelocity(float vel)
	{
		vel = Mathf.DegToRad(vel);

		Vector3 velocity = new(vel, 0, 0);
		
		Thighs.left.AngularVelocity = velocity;
		Thighs.right.AngularVelocity = velocity;
	}
	
	private void SetShinsAngularVelocity(float vel)
	{
		vel = Mathf.DegToRad(vel);

		Vector3 velocity = new(vel, 0, 0);
		
		Shins.left.AngularVelocity = velocity;
		Shins.right.AngularVelocity = velocity;
	}
}