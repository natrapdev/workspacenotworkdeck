using Godot;
using System;

using MyFirst3DGame.scenes.characters.states;
using System.Reflection.Metadata;

namespace MyFirst3DGame.Items;

public partial class InteractableItem : Node3D
{
	[Export] public float InteractDistance { get; set; } = 2f;
	[Export] public Sprite3D TooltipSprite { get; set; }
    [Export] public Vector3 HandleOffset { get; set; } = Vector3.Zero; // Where the player interacts with the item

	public bool BeingUsed { get; set; } = false;
	const float MAXIMUM_LOOK_OFFSET = 30f;

	public void ToggleTooltip(bool show)
	{
		TooltipSprite.Visible = show && !BeingUsed;
	}

    /// <summary>
    /// Checks if a character is close to an item and looking close enough to the item. 
    /// </summary>
    /// <param name="character"></param>
    /// <param name="headBoneAttachment"></param>
    /// <returns></returns>
	public virtual bool CanInteract(Node3D character, BoneAttachment3D headBoneAttachment)
	{
		bool isCloseEnough = DistanceBetween(character) <= InteractDistance;
		bool isBeingLookedAt = LookDifference(character, headBoneAttachment) <= MAXIMUM_LOOK_OFFSET;

		return isCloseEnough && isBeingLookedAt;
	}

	public virtual float DistanceBetween(Node3D character)
	{
        Vector3 position = GlobalPosition + HandleOffset;
		return (position - character.GlobalPosition).Length();
	}

    /// <summary>
    /// Returns the angle between the direction the head is facing and the direction to the item from the head.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="headBoneAttachment"></param>
    /// <returns></returns>
	public virtual float LookDifference(Node3D character, BoneAttachment3D headBoneAttachment)
	{
		Vector3 headPosition = headBoneAttachment.GlobalPosition;
		Vector3 headForward = headBoneAttachment.GlobalBasis.Z;
        Vector3 targetPosition = GlobalPosition + HandleOffset;
		
		Vector3 direction = (targetPosition - headPosition).Normalized();
		float angle = headForward.AngleTo(direction);
        // float angle = headForward.Dot(direction);

		return Mathf.Abs(Mathf.RadToDeg(angle));
	}
}
