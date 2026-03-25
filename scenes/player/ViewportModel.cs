using Godot;
using System.Collections.Generic;
using MyFirst3DGame.scenes.characters.states;
using GodotPlugins.Game;

namespace Viewport;

public partial class ViewportModel : Node3D
{
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public CharacterBody3D Player { get; set; }
	[Export] public Skeleton3D Skeleton { get; set; }
	[Export] public Camera3D MainCamera { get; set; }
	[Export] public ViewportAnimator Animator { get; set; }
	[Export] public ViewportCameraController ViewportCamera { get; set; }

	private WeaponInventory _weaponInventory;

	public override void _Ready()
	{
		_weaponInventory = Humanoid.WeaponInventory;

		MeshInstance3D armModel = new();
		armModel.SetLayerMaskValue(1, false);
		armModel.SetLayerMaskValue(2, true);

		if ((Player as Player).AppearanceSet == 1)
		{
			armModel.Mesh = GD.Load<ArrayMesh>("res://assets/meshes/viewport/man_arms.res");
		}
		else
		{
			GD.PrintErr("Player does not have a valid AppearanceSet");
		}

		Skeleton.AddChild(armModel);
		armModel.Skeleton = Skeleton.GetPath();
		ViewportCamera.MainCamera = MainCamera;
	}

	public void Update()
	{
		State currentState = Humanoid.CurrentState;

		string animation = currentState.Animation;
		Animator.PlaySpeed = Humanoid.Animator.BodyAnimationSpeed;
		Animator.SetAnimation(animation);

		// ViewportCamera.Update();
		SetArmsPosition();
	}

	private void SetArmsPosition()
	{
		Transform3D targetTransform = MainCamera.GlobalTransform;
		GlobalTransform = targetTransform;
	}

	public string GetPathToRightHandWeaponSlot(Node origin)
	{
		return origin.GetPathTo(Skeleton.GetNode<Node3D>("RightHandAttachment/Container"));
	}

	public string GetPathToPrimaryWeaponSlot(Node3D origin)
	{
		return origin.GetPathTo(Skeleton.GetNode<BoneAttachment3D>("RightHandAttachment"));
	}
}
