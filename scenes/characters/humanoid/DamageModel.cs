using Godot;
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;
using MyFirst3DGame.Items;
using Godot.Collections;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class DamageModel : Node3D
{
    [Export] public HumanoidModel Humanoid { get; set; }
    [Export] public CirculationSystem Circulation { get; set; }
    [Export] public HumanoidLimbs LimbCollection { get; set; }
    [Export] public float BodyMass { get; set; } = 68f;

    public readonly System.Collections.Generic.Dictionary<string, float> BodyPartMassCoefficients = new()
    {
        {
            "head", 0.0726f
        },
        {
            "neck", 0.02f
        }, // .01kg taken from the head
        {
            "thorax", 0.2010f
        },
        {
            "abdomen", 0.1310f
        },
        {
            "pelvis", 0.1370f
        },
        {
            "upper arm", 0.0325f
        }, // We usually have two of these so multiply by 2 for total
        {
            "forearm", 0.0187f - 0.0006f
        }, // All body parts add up to 1.0006 so this is to make it consistent
        {
            "hand", 0.0065f
        },
        {
            "thigh", 0.1050f
        },
        {
            "shin", 0.0475f
        },
        {
            "foot", 0.0143f
        }
    };

    public Skeleton3D Skeleton;
    public HumanoidResource Resource;

    public readonly List<InjurySeverity> Severities =
    [
        new("negligible", 1),
        new("minimal", 0.8f),
        new("moderate", 0.6f, 1.1f),
        new("serious", 0.4f, 1.3f),
        new("critical", 0.2f, 1.5f)
    ];

    public List<DamageModule> DamageModules { get; } = new(16);
    public System.Collections.Generic.Dictionary<string, float> BodyPartBleedMultiplier { get; } = new(11)
    {
        {"hand", 0.45f},
        {"foot", 0.45f},
        {"thorax", 1.1f},
        {"abdomen", 1.1f},
        {"pelvis", 1.1f},
        {"upper arm", 1.025f},
        {"forearm", 1.025f}, 
        {"thigh", 1.05f},
        {"shin", 1.05f},
        {"head", 1.5f},
        {"neck", 3f}
    };

    private Node _materials;
    private Dictionary _materialData;

    private BoneAttachment3D _collider;

    public override void _Ready()
    {
        Skeleton = Humanoid.Skeleton;
        Resource = Humanoid.Resource;
        _materials = GetNode<Node>("/root/Materials");
        _materialData = (Dictionary)_materials.Get("material_data");

        LimbCollection.Initialize();
    }

    public void Update(float delta)
    {
        Circulation.Update(delta);
    }

    private int FindLimbIndex(string limbName)
    {
        for (int i = 0; i < DamageModules.Count; i++)
        {
            if (DamageModules[i].Name.Equals(limbName))
            {
                return i;
            }
        }

        return -1;
    }

    private int GetSeverityIndex(DamageModule damageModule)
    {
        float remainingBloodFactor = damageModule.BloodVolume / damageModule.MaxBloodVolume;
        return Severities.FindLastIndex(x => x.Treshold >= remainingBloodFactor);
    }

    private string GetSeverityName(DamageModule damageModule)
    {
        int index = GetSeverityIndex(damageModule);
        return Severities[index].Name;
    }

    private float GetSeverityMultiplier(DamageModule damageModule)
    {
        int index = GetSeverityIndex(damageModule);

        return Severities[index].Multiplier;
    }

    public (float, float) GetHitEffects(HitInfo hitInfo, Limb hitLimb)
    {
        (float impact, float energy) = (
            GetImpactDepth(hitInfo, hitLimb),
            GetKineticEnergyTransfer(hitInfo, hitLimb)
        );
        return (impact, energy);
    }

    private float GetKineticEnergyTransfer(HitInfo hitInfo, Limb hitLimb)
    {
        float energyAbsorption = GetEnergyAbsorption(
            GetMaterialEnergyAbsorption(hitLimb.Material),
            GetImpactDepth(hitInfo, hitLimb),
            Humanoid.Combat.GetEffectiveThicknessOfHit(hitInfo)
        );
        
        Weapon weapon = hitInfo.WeaponNode;
        float weaponSharpnessDivisor = GetWeaponSharpnessDivisor(hitInfo.WeaponNode);
        
        float impactEnergy = GetKineticEnergy(
            weapon.Mass,
            hitInfo.WeaponVelocity.Length(),
            weaponSharpnessDivisor
        );
        
        float impactDepth = GetImpactDepth(
            hitInfo,
            hitLimb
            );
        
        float finalWeaponVelocity = GetFinalVelocity(
            weapon.Mass,
            hitInfo.WeaponVelocity.Length()
        );
        
        float finalEnergy = GetKineticEnergy(
            weapon.Mass,
            finalWeaponVelocity,
            weaponSharpnessDivisor
        );

        float transferredKineticEnergy = impactEnergy - finalEnergy;

        return ApplyEnergyAbsorption(transferredKineticEnergy, energyAbsorption);
    }

    private float GetImpactDepth(HitInfo hitInfo, Limb hitLimb)
    {
        float impactDepth = GetPerforation(
            hitInfo.EffectiveWeaponLength,
            GetDensity(hitInfo.WeaponNode.Material),
            GetDensity(hitLimb.Material, hitLimb.Thickness)
        );

        return impactDepth;
    }

    public float GetImpactDepthRatio(HitInfo hitInfo, Limb hitLimb)
    {
        float depth = GetImpactDepth(hitInfo, hitLimb);
        float thickness = Humanoid.Combat.GetEffectiveThicknessOfHit(hitInfo);

        return depth / thickness;
    }

    private static float GetEnergyAbsorption(float absorption, float impactDepth, float hitThicknessLos)
    {
        return absorption - impactDepth / hitThicknessLos;
    }

    private static float GetInflictedKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor, float energyAbsorption)
    {
        return ApplyEnergyAbsorption(GetKineticEnergy(workingMass, impactVelocity, sharpnessFactor), energyAbsorption);
    }
    
    private static float ApplyEnergyAbsorption(float baseEnergy, float energyAbsorption)
    {
        return baseEnergy / (1 - energyAbsorption);
    }

    private static float GetKineticEnergy(float mass, float velocity, float sharpnessFactor)
    {
        return mass / 2 * Mathf.Pow(velocity / sharpnessFactor, 2);
    }

    private static float GetFinalVelocity(float weaponMass, float impactVelocity)
    {
        // targetMass and targetVelocity are PLACEHOLDERS
        float targetMass = 68;
        float targetVelocity = 0;
        return (weaponMass * impactVelocity + targetMass * targetVelocity) / (weaponMass + targetMass);
    }

    private static float GetVelocityAtImpactAngle(float impactVelocity, float impactAngle)
    {
        return Mathf.Abs(impactVelocity * Mathf.Cos(impactAngle));
    }

    private static float GetPerforation(float workingLength, float workingDensity, float targetDensity)
    {
        return workingLength * (workingDensity / targetDensity);
    }

    private float GetMaterialEnergyAbsorption(string material)
    {
        return (float)((Dictionary)_materialData[material])["absorption"];
    }

    private float GetDensity(string material, float thickness)
    {
        float colliderDensity = GetDensity(material);
        if (material.Equals("gambeson")) colliderDensity *= thickness;

        return colliderDensity;
    }

    private float GetDensity(string material) => (float)((Dictionary)_materialData[material])["density"];

    private static float GetThicknessInLineOfSight(float impactAngle, float thickness)
    {
        return Mathf.Abs(thickness / Mathf.Cos(impactAngle));
    }

    private static float GetWeaponSharpnessDivisor(Weapon weapon) => Mathf.Pow(weapon.Sharpness, 1.15f) + 0.912f;
}