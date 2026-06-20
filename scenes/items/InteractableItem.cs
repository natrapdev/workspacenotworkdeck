using Godot;
using System;

using MyFirst3DGame.scenes.characters.states;
using System.Reflection.Metadata;

namespace MyFirst3DGame.Items;

public partial class InteractableItem : Node3D
{
	[Export] public float InteractDistance { get; set; } = 2.5f;
	[Export] public Sprite3D TooltipSprite { get; set; }
	[Export] public Vector3 HandleOffsetPosition { get; set; } = Vector3.Zero; // Where the player interacts with the item
	[Export] public Vector3 HandleOffsetRotation { get; set; } =  Vector3.Zero;
	
	public bool BeingUsed { get; set; } = false;
	private const float MaxLookOffset = 25f;

	public void ToggleTooltip(bool show) => TooltipSprite.Visible = show && !BeingUsed;

	/// <summary>
	/// Checks if a character is close to an item and looking close enough to the item. 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="headBoneAttachment"></param>
	/// <returns></returns>
	public virtual bool CanInteract(CharacterBody3D character, BoneAttachment3D headBoneAttachment)
	{
		bool isCloseEnough = DistanceBetween(character) <= InteractDistance;
		bool isBeingLookedAt = LookDifference(character, headBoneAttachment) <= MaxLookOffset;

		return isCloseEnough && isBeingLookedAt;
	}
	public virtual bool CanInteract(CharacterBody3D character, Node3D lookNode)
	{
		bool isCloseEnough = DistanceBetween(character) <= InteractDistance;
		bool isBeingLookedAt = LookDifference(character, lookNode) <= MaxLookOffset;

		return isCloseEnough && isBeingLookedAt;
	}

	public virtual float DistanceBetween(CharacterBody3D character)
	{
		Vector3 position = GlobalPosition + HandleOffsetPosition;
		return (character.GlobalPosition - position).Length();
	}

	/// <summary>
	/// Returns the angle between the direction the head is facing and the direction to the item from the head.
	/// </summary>
	/// <param name="character"></param>
	/// <param name="headBoneAttachment"></param>
	/// <returns></returns>
	public virtual float LookDifference(CharacterBody3D character, BoneAttachment3D headBoneAttachment)
	{
		Vector3 headPosition = headBoneAttachment.GlobalPosition;
		Vector3 headForward = headBoneAttachment.GlobalBasis.Z;
		Vector3 targetPosition = GlobalPosition + HandleOffsetPosition;

		Vector3 direction = (targetPosition - headPosition).Normalized();
		float angle = headForward.AngleTo(direction);
		// float angle = headForward.Dot(direction);

		return Mathf.Abs(Mathf.RadToDeg(angle));
	}
	public virtual float LookDifference(CharacterBody3D character, Node3D lookNode)
	{
		Vector3 headPosition = lookNode.GlobalPosition;
		Vector3 headForward = lookNode.GlobalBasis.Z;
		Vector3 targetPosition = GlobalPosition + HandleOffsetPosition;

		Vector3 direction = (targetPosition - headPosition).Normalized();
		// direction = headPosition.DirectionTo(targetPosition);
		float angle = headForward.AngleTo(direction);
		// float angle = Mathf.Acos(headForward.Dot(direction));

		return Mathf.RadToDeg(angle);
	}
}
