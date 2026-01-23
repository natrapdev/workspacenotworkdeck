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

	public readonly Vector3 CameraOffset = new(0, 0, .33f);

	private Skeleton3D _skeleton;
	private CharacterState _characterStates;
	private StateModel _characterStateModel;
	private Camera3D _camera;

	public override void _Ready()
	{
		_characterStateModel = HumanoidNode.GetNode<StateModel>("StateModel");
		_camera = CameraPivot.GetChild<Camera3D>(0);
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
		CameraPivot.GetChild<Camera3D>(0).GlobalPosition = headGlobalTransform.Basis * CameraOffset + headGlobalTransform.Origin;

		TurnHeadWithCamera();
	}

	private void TurnHeadWithCamera()
	{
		Vector3 lookAtPosition = Quaternion.FromEuler(CameraPivot.Rotation) * new Vector3(0, 0, 10);
		HumanoidNode.MoveHeadLookAtTarget(lookAtPosition);
	}
}
