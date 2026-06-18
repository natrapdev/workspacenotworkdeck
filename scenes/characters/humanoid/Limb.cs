using BenchmarkDotNet.Reports;
using Godot;
using MyFirst3DGame.scenes.characters.states;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class Limb : BoneAttachment3D
{
	[Export] public float BleedMultiplier { get; set; } = 1;
	[Export] public string LimbName { get; set; }
	[Export] public float BoneStrength { get; set; } = 400f;
	
	/// <summary>
	/// Defines how much resistance force, in newtons, the <c>Limb</c> will exert on anything trying to pierce it.
	/// <br />Can be affected by sharpness of object.
	/// <br /><br /> For reference, see how much resistance force the following materials have:
	/// </summary>
	/// <list type="bullet">
	///	<item><description>Skin = 35-55N</description></item>
	///	<item><description>Fat/Muscle = 80-140N</description></item>
	///	<item><description>Bone/Cartilage = 150-550N</description></item>
	/// </list>
	[Export] public float CutResistance { get; set; } = 90f;
	
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
	public string Material { get; private set; } = "flesh";
	public float RemainingBloodRatio { get { return CurrentBloodVolume / MaxBloodVolume; } }
	public float CurrentBloodVolume { get; set; }

	private float _currentPhysicalVolume;
	private float _currentBleedRate;
	private bool _isBoneBroken;

	public void Initialize()
	{
		CurrentBloodVolume = MaxBloodVolume;
		_currentPhysicalVolume = PhysicalVolume;
		UseExternalSkeleton = true;

		if (Model.Humanoid.GetParent().Name == "Player") return;

		Label3D bloodLabel = new()
		{
			Text = "100%",
			Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
			PixelSize = 0.00125f,
			NoDepthTest = true,
			Name = "Label"
		};
		AddChild(bloodLabel);

		bloodLabel.Position = ((CollisionShape3D)GetChild(0).GetChild(0)).Position;
	}

	public void Hit(HitInfo hitInfo)
	{
		(float impactDepth, float impactEnergy) = Model.GetHitEffects(hitInfo, this);

		float cutTime = impactDepth / hitInfo.WeaponVelocity.Length();
		float cutArea = cutTime * hitInfo.WeaponVelocity.Length();
		float cutVolume = cutArea * cutTime * impactDepth;
		
		_currentPhysicalVolume -= cutVolume;
		
		float bloodLossFactor = 1 - _currentPhysicalVolume / PhysicalVolume;
		float immediateBloodLoss = MaxBloodVolume * bloodLossFactor * cutArea;
		
		CurrentBloodVolume = Mathf.Clamp(CurrentBloodVolume - immediateBloodLoss, 0, MaxBloodVolume);
		CirculationSystem.CurrentBloodVolume -= Mathf.Min(immediateBloodLoss, MaxBloodVolume);
		_currentBleedRate += MaxBloodVolume * bloodLossFactor;
		
		GD.Print($"\nHit {LimbName} ({BoneStrength}) \nimpact energy of {impactEnergy} joules \nat an angle of {Mathf.RadToDeg(hitInfo.HitAngle)} degrees.\n");
		GD.Print($"Cut area: {cutArea} m^2\nCut volume: {cutVolume} m^3\n\nDepth ratio: {Model.GetImpactDepthRatio(hitInfo, this)}\nImmediate blood loss: {immediateBloodLoss} mL\nbloodLossFactor: {bloodLossFactor * 100}%\n");
		
		if (impactEnergy > BoneStrength) BreakBone();
		
		var label = GetNodeOrNull<Label3D>("Label");
        
		
		if (label is not null) label.Text = $"{RemainingBloodRatio * 100:F2}%";
		
	}

	private void BreakBone()
	{
		GD.Print($"Broke bone of {LimbName}");
		_isBoneBroken = true;
	}
}