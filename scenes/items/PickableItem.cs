using Godot;
using System;

using MyFirst3DGame.scenes.characters.states;

namespace MyFirst3DGame.Items;

public partial class PickableItem : InteractableItem
{
	[Export] public bool Anchored { get; set; }
	[Export] public RigidBody3D PhysicalBody { get; set; }
	[Export] public MeshInstance3D ActualMesh { get; set; }
	[Export] public MeshInstance3D PhysicalMesh { get; set; }
	public Node3D Parent { get; set; }

	public bool IsPickedUp = false;
	public override void _PhysicsProcess(double delta)
	{
		PhysicalBody.Freeze = Anchored || IsPickedUp;
		ActualMesh.Visible = IsPickedUp;
		PhysicalBody.Visible = !IsPickedUp;

		if (!IsPickedUp)
		{
			PhysicalBody.CollisionLayer = 1;
		}
		else
		{
			PhysicalBody.CollisionLayer = 2;
			PhysicalBody.Position = Vector3.Zero;
			PhysicalBody.Visible = false;
		}
	}

	public virtual void PickedUp(Humanoid humanoid)
	{
		IsPickedUp = true;
		Inventory inventory = humanoid.Inventory;
		PhysicalBody.AddCollisionExceptionWith(humanoid.Character);
		inventory.AddItemToInventory(this);
		PhysicalBody.CollisionMask = 2;
	}
}
