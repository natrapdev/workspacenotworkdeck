using Godot;
using System;

public partial class CharacterSkeleton : Skeleton3D
{
	[Export] private PhysicalBoneSimulator3D PhysicalBones { get; set; }
	[Export] private Boolean Ragdolled { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Ragdolled = true;
		//PhysicalBones.PhysicalBonesStartSimulation();
	}
}