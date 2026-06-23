using Godot;
using System.Collections.Generic;
using MyFirst3DGame.scenes.characters.states;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.player;

public partial class ViewportModel : Node3D
{
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public CharacterBody3D Player { get; set; }
	[Export] public Skeleton3D Skeleton { get; set; }
	[Export] public Camera3D MainCamera { get; set; }
	[Export] public ViewportAnimator Animator { get; set; }
	[Export] public ViewportCameraController ViewportCamera { get; set; }
	[Export] public ViewportTiltTracker TiltTracker { get; set; }

	private WeaponInventory _weaponInventory;

	public readonly Dictionary<string, Vector2> AttackVectors = new()
	{
		{"slash1", new Vector2(-1, 0)},
		{"slash2", new Vector2(1, 0)},
		{"slash3", new Vector2(0, -1)},
		{"thrust", new Vector2(0, 0)}
	};

	public override void _Ready()
	{
		_weaponInventory = Humanoid.WeaponInventory;

		MeshInstance3D armModel = new();
		armModel.SetLayerMaskValue(1, false);
		armModel.SetLayerMaskValue(2, true);

		if (((Player)Player).AppearanceSet == 1) // wok alert (1 = male, 0 = female)
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

	public void Update(InputPackage input, float delta)
	{
		State currentState = Humanoid.CurrentState;

		string animation = currentState.Animation;
		Animator.PlaySpeed = Humanoid.Animator.BodyAnimationSpeed;
		Animator.AnimationPlayer.SetSpeedScale(Humanoid.Animator.GetBodySpeedScale());
		Animator.SetAnimation(animation);
		
		// ViewportCamera.Update();
		SetArmsPosition();

		if (currentState.RightWeaponHurts())
		{
			TiltTracker.StartTracking();
		}
		else
		{
			TiltTracker.StopTracking();
		}

		int index = currentState.StateName.IndexOf('_');
		string baseStateName =
			index != -1 ? currentState.StateName[..index] : currentState.StateName;

		TiltTracker.UpdateTilt(
			delta,
			AttackVectors.GetValueOrDefault(baseStateName, Vector2.Zero)
		);
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
	
	public string GetPathToLeftHandWeaponSlot(Node origin)
	{
		return origin.GetPathTo(Skeleton.GetNode<Node3D>("LeftHandAttachment/Container"));
	}
}
