using Godot;
using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using MyFirst3DGame.scenes.characters.states;

public partial class CameraController : Node3D
{
	[Export] public CharacterBody3D Player { get; set; }
	[Export] public Node3D CharacterNode { get; set; } // This is the character model
	[Export] public float CameraPanSpeed { get; set; }
	[Export] public Camera3D Camera { get; set; }
	[Export] public float HorizontalLimit { get; set; } = 75f;
	[Export] public float VerticalLimit { get; set; } = 60f;
	[Export] public HumanoidModel Humanoid { get; set; }

	private Vector3 _cameraRotation = Vector3.Zero;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			Vector2 delta = motionEvent.Relative;

			float newX = RotationDegrees.X + (delta.Y * CameraPanSpeed);
			float newY = RotationDegrees.Y + (-delta.X * CameraPanSpeed);

			newX = Mathf.Clamp( // Vertical clamp
				newX,
				-VerticalLimit,
				VerticalLimit
			);

			// newY = Mathf.Clamp( // Horizontal clamp
			// 	newY,
			// 	-HorizontalLimit,
			// 	HorizontalLimit
			// );

			RotationDegrees = new Vector3(newX, newY, 0);
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