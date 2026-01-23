using Godot;
using System;

using MyFirst3DGame.scenes.characters.states;

namespace MyFirst3DGame.Items;

public partial class PickableItem : Node3D
{
	[Export] public bool Anchored { get; set; } = false;
	[Export] public float PickUpDistance { get; set; } = 2f;
	[Export] public Sprite3D ItemBillboardSprite { get; set; }
	[Export] public RigidBody3D PhysicalBody { get; set; }
	[Export] public MeshInstance3D ActualMesh { get; set; }
	[Export] public MeshInstance3D PhysicalMesh { get; set; }

	public bool IsPickedUp = false;
	const float MAXIMUM_LOOK_OFFSET = 500f;

	public override void _PhysicsProcess(double delta)
	{
		PhysicalBody.Freeze = Anchored;
		ActualMesh.Visible = IsPickedUp;
		PhysicalBody.Visible = !IsPickedUp;

		if (!IsPickedUp)
		{
			ActualMesh.GlobalPosition = PhysicalBody.GlobalPosition;
		}
		else
		{
			PhysicalBody.Position = Vector3.Zero;
			ActualMesh.Position = Vector3.Zero;
			Visible = false;
		}
	}

	public void TogglePickUpTooltip(bool show)
	{
		ItemBillboardSprite.Visible = show && !IsPickedUp;
	}

	public bool CanBePickedUp(Node3D character, BoneAttachment3D headBoneAttachment)
	{
		bool isCloseEnough = DistanceFromItem(character) <= PickUpDistance;
		bool isBeingLookedAt = HeadItemDirectionDifference(character, headBoneAttachment) <= MAXIMUM_LOOK_OFFSET;

		return isCloseEnough && isBeingLookedAt;
	}

	public float DistanceFromItem(Node3D character)
	{
		return (PhysicalBody.GlobalPosition - character.GlobalPosition).Length();
	}

	public float HeadItemDirectionDifference(Node3D character, BoneAttachment3D headBoneAttachment)
	{
		Vector3 headPosition = headBoneAttachment.GlobalPosition;
		Vector3 headForward = headBoneAttachment.GlobalBasis.Z;
		
		Vector3 direction = (PhysicalBody.GlobalPosition - headPosition).Normalized();
		float angle = headForward.AngleTo(direction);

		return Mathf.Abs(Mathf.RadToDeg(angle));
	}
}
