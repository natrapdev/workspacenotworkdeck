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
    [Export] public HumanoidLimbs Limbs { get; set; }
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
        }, // All body parts add up to 1.0006 so this is to make it consitent
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
    public System.Collections.Generic.Dictionary<string, float> BodyPartBleedMultiplier { get; } = new()
    {
        {
            "hand", 0.45f
        },
        {
            "foot", 0.45f
        },
        {
            "thorax", 1.1f
        },
        {
            "abdomen", 1.1f
        },
        {
            "pelvis", 1.1f
        },
        {
            "upper arm", 1.025f
        },
        {
            "forearm", 1.025f
        },
        {
            "thigh", 1.05f
        },
        {
            "shin", 1.05f
        },
        {
            "head", 1.5f
        },
        {
            "neck", 3f
        }
    };

    private Node _materials;
    private Dictionary _materialData;

    private BoneAttachment3D _collider;

    public void OnReady()
    {
        Skeleton = Humanoid.Skeleton;
        Resource = Humanoid.Resource;
        _materials = GetNode<Node>("/root/Materials");
        _materialData = (Dictionary)_materials.Get("material_data");

        Limbs.Initialize();
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

    public override void _Process(double delta)
    {
        // for (int i = 0; i < DamageModules.Count; i++)
        // {
        //     DamageModule limb = DamageModules[i];
        //     float bleedMultiplier = BodyPartBleedMultiplier.GetValueOrDefault(limb.Name, 1);
        //     float lostBlood = limb.BleedRate * bleedMultiplier * (float)delta;
        //     limb.BloodVolume = Mathf.Clamp(limb.BloodVolume - lostBlood, 0, limb.MaxBloodVolume);
        //     DamageModules[i] = limb;
        //
        //     if (!limb.Name.Equals("Neck")) CheckForEffect(DamageModules[i]);
        // }
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

    private void CheckForEffect(in DamageModule damageModule)
    {
        float remainingBloodFactor = damageModule.BloodVolume / damageModule.MaxBloodVolume;

        string label = GetSeverityName(damageModule);
        float multiplier = GetSeverityMultiplier(damageModule);

        if (damageModule.Parent.GetChild(0).GetChild(0) is CollisionShape3D collisionShape)
        {
            byte r = 0, g = 255, b = 0;

            if (remainingBloodFactor > .66f)
            {
                // r = (byte)(255 * (1 - remainingBloodFactor));
                r = 0;
                g = 255;
            }
            else if (remainingBloodFactor > .33f)
            {
                r = 255;
                // g = (byte)(255 * remainingBloodFactor);
                g = 0;
            }
            else if (remainingBloodFactor <= .33f)
            {
                // r = (byte)(255 * remainingBloodFactor);
                r = 0;
                g = 0;
            }
            else
            {
                r = 0;
                g = 0;
            }

            Color color = Color.Color8(r, g, b);

            collisionShape.DebugColor = color;
        }
    }

    public (float, float) GetHitEffects(HitInfo hitInfo, Limb hitLimb)
    {
        return (
            GetImpactDepth(hitInfo, hitLimb),
            GetKineticEnergyTransfer(hitInfo, hitLimb)
        );
    }

    private float GetKineticEnergyTransfer(HitInfo hitInfo, Limb hitLimb)
    {
        Vector3 hitDirection = (hitInfo.HitPosition - hitInfo.WeaponHitSource).Normalized();
        Weapon weapon = hitInfo.WeaponNode;
        float weaponSharpnessDivisor = weapon.Sharpness * 1.25f + 1;
        
        float impactAngle = GetImpactAngleRadians(
            hitDirection,
            hitInfo.HitNormal
            );
        float impactDepth = GetImpactDepth(
            hitInfo,
            hitLimb
            );
        
        float workDone = GetWorkDone(
            hitLimb.CutResistance,
            impactDepth
            );
        float impactEnergy = GetKineticEnergy(
            weapon.Mass,
            hitInfo.WeaponVelocity.Length(),
            weaponSharpnessDivisor
            );
        float finalWeaponVelocity = GetFinalVelocity(
            impactEnergy,
            weapon.Mass,
            workDone
            );

        float effectiveEnergyAbsorption = GetEffectiveEnergyAbsorption(
            GetEnergyAbsorption(hitLimb.Material),
            GetImpactDepth(hitInfo, hitLimb),
            GetThicknessInLineOfSight(impactAngle, hitLimb.Thickness)
        );

        float inflictedKineticEnergy = GetKineticEnergy(
            weapon.Mass,
            hitInfo.WeaponVelocity.Length(), 
            finalWeaponVelocity,
            weaponSharpnessDivisor
            );

        return ApplyEnergyAbsorption(inflictedKineticEnergy, effectiveEnergyAbsorption);
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
        Vector3 hitDirection = (hitInfo.HitPosition - hitInfo.WeaponHitSource).Normalized();
        float depth = GetImpactDepth(hitInfo, hitLimb);
        float thickness = GetThicknessInLineOfSight(
            GetImpactAngleRadians(hitDirection, hitInfo.HitNormal),
            hitLimb.Thickness
        );

        return depth / thickness;
    }

    private static float GetEffectiveEnergyAbsorption(float absorption, float impactDepth, float hitThicknessLos)
    {
        return absorption - impactDepth / hitThicknessLos;
    }

    private static float GetInflictedKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor, float energyAbsorption)
    {
        return ApplyEnergyAbsorption(GetKineticEnergy(workingMass, impactVelocity, sharpnessFactor), energyAbsorption);
    }
    
    private static float ApplyEnergyAbsorption(float baseEnergy, float energyAbsorption)
    {
        return baseEnergy * (1 - energyAbsorption);
    }

    private static float GetKineticEnergy(float weaponMass, float impactVelocity, float sharpnessFactor)
    {
        return .5f * weaponMass * Mathf.Pow(impactVelocity / sharpnessFactor, 2);
    }

    private static float GetKineticEnergy(float weaponMass, float initialVelocity, float finalVelocity, float sharpnessFactor)
    {
        float baseEnergy = .5f * weaponMass * (Mathf.Pow(initialVelocity, 2) - Mathf.Pow(finalVelocity, 2));
        return baseEnergy / sharpnessFactor;
    }

    private static float GetWorkDone(float targetResistanceNewtons, float cutDepthMetres)
    {
        return targetResistanceNewtons * cutDepthMetres;
    }

    private static float GetFinalVelocity(float impactEnergy, float weaponMass, float workDone)
    {
        return Mathf.Sqrt(2 * (impactEnergy - workDone) / weaponMass);
    }

    private static float GetVelocityAtImpactAngle(float impactVelocity, float impactAngle)
    {
        return Mathf.Abs(impactVelocity * Mathf.Cos(impactAngle));
    }

    private static float GetPerforation(float workingLength, float workingDensity, float targetDensity)
    {
        return workingLength * (workingDensity / targetDensity);
    }

    private float GetEnergyAbsorption(string material)
    {
        return (float)((Dictionary)_materialData[material])["absorption"];
    }

    private static float GetImpactAngleDegrees(Vector3 hitDirection, Vector3 hitNormal)
    {
        return Mathf.RadToDeg(Mathf.Acos(hitDirection.Dot(hitNormal)));
    }

    private static float GetImpactAngleRadians(Vector3 hitDirection, Vector3 hitNormal)
    {
        return Mathf.Acos(hitDirection.Dot(hitNormal));
    }

    private float GetDensity(string material, float thickness)
    {
        float colliderDensity = GetDensity(material);
        if (material.Equals("gambeson")) colliderDensity *= thickness;

        return colliderDensity;
    }

    private float GetDensity(string material)
    {
        return (float)((Dictionary)_materialData[material])["density"];
    }

    private static float GetThicknessInLineOfSight(float impactAngle, float thickness)
    {
        return Mathf.Abs(thickness / Mathf.Cos(impactAngle));
    }
}