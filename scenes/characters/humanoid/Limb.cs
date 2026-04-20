using Godot;
using MyFirst3DGame.scenes.characters.states;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class Limb : BoneAttachment3D
{
	[Export] public float BleedMultiplier { get; set; } = 1;
	[Export] public string LimbName { get; set; }
	[Export] public float BoneStrength { get; set; } = 400f;
	public CirculationSystem CirculationSystem { get; set; }
	public DamageModel Model { get; set; }
	public List<Limb> Neighbours { get; set; } = new(4);
	public Area3D DetectionArea { get; set; }
	public Skeleton3D Skeleton { get; set; }
	public float PhysicalVolume { get; set; }
	public float MaxBloodVolume { get; set; }
	public float Mass { get; set; }
	public float CurrentBleedRate { get { return _currentBleedRate * BleedMultiplier; } }
	public float Thickness { get; set; }
	public string Material { get; set; } = "flesh";
	public float RemainingBloodRatio { get { return _currentBleedRate / MaxBloodVolume; } }

	public float CurrentBloodVolume { get; set; }

	private float _currentPhysicalVolume;
	private float _currentBleedRate = 0f;
	private bool _isBoneBroken = false;

	public void Initialize()
	{
		CurrentBloodVolume = MaxBloodVolume;
		_currentPhysicalVolume = PhysicalVolume;
	}

	public void Hit(HitInfo hitInfo)
	{
		(float impactDepth, float impactEnergy) = Model.GetHitEffects(hitInfo, this);

		float cutTime = impactDepth / hitInfo.WeaponVelocity.Length();
		float cutArea = cutTime * hitInfo.WeaponVelocity.Length() * impactDepth;
		float cutVolume = cutArea * cutTime * impactDepth;
		float bloodLossFactor = cutVolume / PhysicalVolume;

		GD.Print($"\nCut area: {cutArea} m^2\nCut volume: {cutVolume} m^3\nBlood loss factor: {bloodLossFactor * 100}%");

		float immediateBloodLoss = MaxBloodVolume * (bloodLossFactor + Model.GetImpactDepthRatio(hitInfo, this));

		CurrentBloodVolume -= immediateBloodLoss;
		CirculationSystem.CurrentBloodVolume -= immediateBloodLoss;
		_currentBleedRate += MaxBloodVolume * bloodLossFactor;

		if (impactEnergy > BoneStrength) BreakBone();
	}

	public void BreakBone()
	{
		_isBoneBroken = true;
	}
}