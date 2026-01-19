using Godot;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;

public partial class CameraController : Node3D
{	
	[Export] public CharacterBody3D Player { get; set; }
	[Export] public Node3D CharacterNode { get; set; }
	[Export] public float CameraPanSpeed { get; set; }
	[Export] public Camera3D Camera { get; set; }

	private const float bodyFollowHeadAngle = 60f;

	public override void _Ready()
	{

	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion motionEvent)
		{
			Vector2 delta = motionEvent.Relative;

			float newY = RotationDegrees.Y + (-delta.X * CameraPanSpeed);
			float newX = RotationDegrees.X + (delta.Y * CameraPanSpeed);

			newX = Mathf.Clamp(newX, -80, 80);

			float cameraCharacterRotationDifference = CharacterNode.RotationDegrees.Y - newY;

			RotationDegrees = new Vector3(newX, newY, 0);

			float angleDifference = (CharacterNode.RotationDegrees.Y - newY + 180) % 360 - 180;
			angleDifference = angleDifference < -180 ? angleDifference + 360 : angleDifference;

			// Body rotation will trail behind where the head is facing after head is turned a certain amount
			if (Player.Velocity == Vector3.Zero)
			{
				float characterRotation;

				if (RotationDegrees.Y < CharacterNode.RotationDegrees.Y)
				{
					characterRotation = Mathf.Clamp(newY + bodyFollowHeadAngle, newY, CharacterNode.RotationDegrees.Y);
				}
				else
				{
					characterRotation = Mathf.Clamp(newY - bodyFollowHeadAngle, CharacterNode.RotationDegrees.Y, newY);
				}

				CharacterNode.RotationDegrees = new Vector3(CharacterNode.RotationDegrees.X, characterRotation, CharacterNode.RotationDegrees.Z);
			}
		}

		if (inputEvent.IsActionPressed("ui_cancel"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
    }
}