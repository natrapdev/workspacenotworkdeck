using Godot;
using System;
using System.Linq;
namespace MyFirst3DGame.scenes.characters.states;

public partial class Player : CharacterBody3D
{
	[Export] public Node3D CharacterModel { get; set; }
	[Export] public Humanoid HumanoidNode { get; set; }
	[Export] public Node InputSource { get; set; }
	[Export] public Node3D CameraPivot { get; set; }

	public readonly Vector3 CameraOffset = new(0, 0, .3f);
	private const float _CameraTowardsOffset = 1f;

	private Skeleton3D _skeleton;
	private CharacterState _characterStates;
	private StateModel _characterStateModel;
	public Camera3D Camera { get; set; }

	public override void _Ready()
	{
		_characterStateModel = HumanoidNode.GetNode<StateModel>("StateModel");
		Camera = CameraPivot.GetChild<Camera3D>(0);
		_skeleton = CharacterModel.GetNode<Skeleton3D>("rig/Skeleton3D");
		LookAtModifier3D headLookAt = _skeleton.GetNode<LookAtModifier3D>("HeadLookAt");
		headLookAt.TargetNode = headLookAt.GetPathTo(HumanoidNode.HeadLookAtTarget);
	}

	public override void _Process(double delta)
	{
		FirstPersonCamera();
		InputPackage input = ((InputGatherer)InputSource).GatherInput();
		_characterStateModel.Update(input, (float)delta);
		MoveAndSlide();
	}

	private void FirstPersonCamera()
	{
		Transform3D headGlobalTransform = _characterStateModel.CharacterResource.GetHeadBoneGlobalTransform();
		Camera.GlobalPosition = headGlobalTransform.Basis * CameraOffset + headGlobalTransform.Origin;

		TurnHeadWithCamera();
	}

	private void TurnHeadWithCamera()
	{
		// Vector3 lookAtPosition = Quaternion.FromEuler(CameraPivot.Rotation) * new Vector3(0, 0, 1);
		Vector3 cameraForward = CameraPivot.GlobalTransform.Basis.Z;
		Vector3 lookAtPosition = Camera.GlobalPosition + (cameraForward * _CameraTowardsOffset);

		HumanoidNode.MoveHeadLookAtTarget(lookAtPosition);
	}
}
