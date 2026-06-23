using Godot;
using MyFirst3DGame.scenes.characters.humanoid;
using MyFirst3DGame.scenes.player;
using MyFirst3DGame.scenes.characters.states;

namespace MyFirst3DGame.scenes;

public partial class Player : CharacterBody3D
{
	[Export] public CharacterAppearance CharacterModel { get; set; }
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public InputGatherer InputSource { get; set; }
	[Export] public CameraController CameraPivot { get; set; }
	[Export] public ViewportModel Viewport { get; set; }
	[Export] public int AppearanceSet { get; set; } = 1; // wok alert

	private readonly Vector3 _cameraOffset = new(0, .1f, .2f);
	private const float CameraTowardsOffset = 5f;

	private Skeleton3D _skeleton;
	private State _characterStates;
	private HumanoidStates _characterStateModel;
	private BoneAttachment3D _headBoneAttachment;
	private Camera3D Camera { get; set; }
	private float _cameraPanSpeed = 1f;

	[Export] public float AttackSensitivityMultiplier { get; set; } = 1f;

	public override void _Ready()
	{
		CharacterModel.AcceptModel(Humanoid);
		_characterStateModel = Humanoid.StateContainer;
		Camera = CameraPivot.GetChild<Camera3D>(0);
		_skeleton = Humanoid.Skeleton;
		_headBoneAttachment = _skeleton.GetNode<BoneAttachment3D>("HeadBoneAttachment");

		if (Viewport is not null)
		{
			Humanoid.WeaponInventory.RightHandWeaponContainerPath = Viewport.GetPathToRightHandWeaponSlot(Humanoid);
			Humanoid.WeaponInventory.LeftHandWeaponContainerPath = Viewport.GetPathToLeftHandWeaponSlot(Humanoid);
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
		Vector3 lookAtPosition = Camera.GlobalPosition + (cameraForward * CameraTowardsOffset);

		Humanoid.MoveHeadLookAtTarget(lookAtPosition);
	}
}
