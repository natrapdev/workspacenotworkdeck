using Godot;
using System;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes;

public partial class CharacterAppearance : Node3D
{
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public MeshInstance3D Body { get; set; }
	[Export] public MeshInstance3D Arms { get; set; }

	public void AcceptModel(HumanoidModel humanoid)
	{
		Humanoid = humanoid;
		Body.Skeleton = humanoid.Skeleton.GetPath();
		Arms.Skeleton = humanoid.Skeleton.GetPath();

		if (!Humanoid.GetParent<CharacterBody3D>().Name.ToString().Contains("Player")) return;
		Body.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
		Arms.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
	}

	private void UpdateWeaponVisuals()
	{

	}
	// AnimationMixer (at: humanoid_model.tscn): 'human_legs_animation_library/walk_front_one_handed', couldn't resolve track:  'rig/Skeleton3D:toe.L'. This warning can be disabled in Project Settings.
	private void UpdateResourceInterface()
	{
		if (Humanoid.Team != 0)
		{

		}
	}
}
