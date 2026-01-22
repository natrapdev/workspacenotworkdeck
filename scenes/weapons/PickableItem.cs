using Godot;
using System;

namespace MyFirst3DGame.Items;

public partial class PickableItem : Node3D
{
	[Export] public bool Anchored { get; set; } = false;
	[Export] public float PickUpDistance { get; set; } = 2f;
	[Export] public Sprite3D ItemBillboardSprite { get; set; }
	[Export] public RigidBody3D PhysicalBody { get; set; }
	[Export] public MeshInstance3D ActualMesh { get; set; }
	[Export] public MeshInstance3D PhysicalMesh { get; set; }

	private bool _isPickedUp = false;
	const float MAXIMUM_LOOK_OFFSET = 0.15f;

	public override void _PhysicsProcess(double delta)
	{
		PhysicalBody.Freeze = Anchored;
		ActualMesh.Visible = _isPickedUp;
		PhysicalBody.Visible = !_isPickedUp;

		if (!_isPickedUp)
		{
			ActualMesh.GlobalPosition = PhysicalBody.GlobalPosition;
		}
		else
		{
			PhysicalBody.Position = Vector3.Zero;
			ActualMesh.Position = Vector3.Zero;
		}
	}

	public void TogglePickUpTooltip(bool show)
	{
		ItemBillboardSprite.Visible = show;
	}

	public bool CanBePickedUp(Node3D character)
	{
		return DistanceFromItem(character) <= PickUpDistance && AngleDifference2D(character) <= MAXIMUM_LOOK_OFFSET;
	}

	public float DistanceFromItem(Node3D character)
	{
		return (PhysicalBody.GlobalPosition - character.GlobalPosition).Length();
	}

	public float AngleDifference2D(Node3D character)
	{
		Vector3 facingDirection = character.GlobalTransform.Basis.Z;
		Vector3 characterPosition = character.GlobalTransform.Origin;

		float angleDifferenceX = Mathf.Abs(characterPosition.DirectionTo(PhysicalMesh.GlobalTransform.Origin).X - facingDirection.X);
		// float angleDifferenceY = Mathf.Abs(characterPosition.DirectionTo(PhysicalMesh.GlobalTransform.Origin).Y - facingDirection.Y);

		return angleDifferenceX;
	}
}
