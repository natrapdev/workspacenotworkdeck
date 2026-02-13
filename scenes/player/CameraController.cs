using Godot;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using MyFirst3DGame.scenes.characters.states;

public partial class CameraController : Node3D
{
	[Export] public CharacterBody3D Player { get; set; }
	[Export] public HumanoidModel CharacterNode { get; set; }
	[Export] public float CameraPanSpeed { get; set; }
	[Export] public Camera3D Camera { get; set; }
	[Export] public float HorizontalLimit { get; set; } = 80f;
	[Export] public float VerticalLimit { get; set; } = 80f;

	private const float bodyFollowHeadAngle = 60f;

	public override void _Ready()
	{

	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			Vector2 delta = motionEvent.Relative;

			float newY = RotationDegrees.Y + (-delta.X * CameraPanSpeed);
			float newX = RotationDegrees.X + (delta.Y * CameraPanSpeed);

			newX = Mathf.Clamp( // Vertical clamp
				newX,
				Player.RotationDegrees.X - VerticalLimit,
				Player.RotationDegrees.X + VerticalLimit
			);

			// newY = Mathf.Clamp( // Horizontal clamp
			// 	newY,
			// 	Player.RotationDegrees.Y - HorizontalLimit,
			// 	Player.RotationDegrees.Y + HorizontalLimit
			// );

			RotationDegrees = new Vector3(newX, newY, 0);

			// float angleDifference = (Player.RotationDegrees.Y - newY + 180) % 360 - 180;
			// angleDifference = angleDifference < -180 ? angleDifference + 360 : angleDifference;

			// // Body rotation will trail behind where the head is facing after head is turned a certain amount
			// if (Player.Velocity == Vector3.Zero)
			// {
			// 	float characterRotation;

			// 	if (RotationDegrees.Y < Player.RotationDegrees.Y)
			// 	{
			// 		characterRotation = Mathf.Clamp(newY + bodyFollowHeadAngle, newY, CharacterNode.RotationDegrees.Y);
			// 	}
			// 	else
			// 	{
			// 		characterRotation = Mathf.Clamp(newY - bodyFollowHeadAngle, CharacterNode.RotationDegrees.Y, newY);
			// 	}

			// 	Player.RotationDegrees = new Vector3(Player.RotationDegrees.X, characterRotation, Player.RotationDegrees.Z);
			// }
		}

		if (@event.IsActionPressed("ui_cancel"))
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