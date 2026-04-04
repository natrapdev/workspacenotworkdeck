using Godot;
using System;
using System.IO;
using System.Linq;
using Viewport;
namespace MyFirst3DGame.scenes.characters.states;

public partial class Player : CharacterBody3D
{
	[Export] public CharacterAppearance CharacterModel { get; set; }
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public InputGatherer InputSource { get; set; }
	[Export] public CameraController CameraPivot { get; set; }
	[Export] public ViewportModel Viewport { get; set; }
	[Export] public int AppearanceSet { get; set; } = 1; // wok alert

	public readonly Vector3 CameraOffset = new(0, .1f, .2f);
	private const float _CameraTowardsOffset = 50f;

	private Skeleton3D _skeleton;
	private State _characterStates;
	private HumanoidStates _characterStateModel;
	public BoneAttachment3D HeadBoneAttachment;
	public Camera3D Camera { get; set; }
	private float _cameraPanSpeed = 1f;

	public float AttackSensitivityMultiplier { get; set; } = .375f;

	public override void _Ready()
	{
		CharacterModel.AcceptModel(Humanoid);
		_characterStateModel = Humanoid.StateContainer;
		Camera = CameraPivot.GetChild<Camera3D>(0);
		_skeleton = Humanoid.Skeleton;
		HeadBoneAttachment = _skeleton.GetNode<BoneAttachment3D>("HeadBoneAttachment");

		if (Viewport is not null)
		{
			string path = Viewport.GetPathToRightHandWeaponSlot(Humanoid);
			Humanoid.WeaponInventory.RightHandWeaponContainerPath = path;
		}

		_cameraPanSpeed = CameraPivot.CameraPanSpeed;
	}

	public override void _Process(double delta)
	{
		FirstPersonCamera();
		InputPackage input = InputSource.GatherInput();

		Humanoid.Update(input, (float)delta);
		Viewport.Update(input, (float)delta);

		MoveAndSlide();

		if (Humanoid.CurrentState.StateName.Contains("slash") || Humanoid.CurrentState.StateName.Contains("thrust"))
		{
			CameraPivot.CameraPanSpeed = _cameraPanSpeed * AttackSensitivityMultiplier;
		}
		else
		{
			CameraPivot.CameraPanSpeed = _cameraPanSpeed;
		}
	}

	private void FirstPersonCamera()
	{
		// Transform3D headGlobalTransform = _characterStateModel.CharacterResource.GetHeadBoneGlobalTransform();
		// Camera.GlobalPosition = headGlobalTransform.Basis * CameraOffset + headGlobalTransform.Origin;

		// Transform3D targetTransform = HeadBoneAttachment.GlobalTransform;
		// Camera.GlobalPosition = targetTransform.Basis * CameraOffset + targetTransform.Origin;

		TurnHeadWithCamera();
	}

	private void TurnHeadWithCamera()
	{
		// Vector3 lookAtPosition = Quaternion.FromEuler(CameraPivot.Rotation) * new Vector3(0, 0, 1);
		Vector3 cameraForward = CameraPivot.GlobalTransform.Basis.Z;
		Vector3 lookAtPosition = Camera.GlobalPosition + (cameraForward * _CameraTowardsOffset);

		Humanoid.MoveHeadLookAtTarget(lookAtPosition);
	}
}
