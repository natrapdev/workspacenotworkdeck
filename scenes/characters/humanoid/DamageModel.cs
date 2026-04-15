using Godot;
using System;
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using MyFirst3DGame.Items;
using Godot.Collections;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class DamageModel : Node3D
{
    [Export] public HumanoidSkeleton Skeleton { get; set; }
    [Export] public HumanoidResource Resource { get; set; }
    [Export] public HumanoidModel Humanoid { get; set; }

    public enum Severity
    {
        Negligable = 1,
        Minimal = 2,
        Moderate = 3,
        Critical = 4,
        Catastrophic = 5
    }

    public readonly System.Collections.Generic.Dictionary<string, float> Severities = new()
    {
        {"negligable", 0f},
        {"minimal", 0.2f},
        {"moderate", 0.4f},
        {"severe", 0.6f},
        {"critical", 0.8f},
        {"catastrophic", 1f}
    };

    public readonly List<float> SeverityThresholds =
    [.05f, .2f, .4f, .6f, .8f, 1f];
    public readonly List<string> SeverityNames =
    ["catastrophic", "critical", "severe", "moderate", "minimal", "none"];

    public List<DamageModule> DamageModules { get; set; } = new(16);
    public System.Collections.Generic.Dictionary<string, float> BodyPartBleedMultiplier { get; } = new()
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

    private HitInfo _lastHitInfo;
    private DamageModule _lastHitLimb;
    private BoneAttachment3D _collider;

    public void OnReady()
    {
        float bodyMass = Resource.BodyMass;
        float totalBloodVolume = Resource.TotalBloodVolume;

        foreach (BoneAttachment3D child in GetChildren().Cast<BoneAttachment3D>())
        {
            string bodyPartName = child.Name;
            string translatedName = TranslateName(bodyPartName);
            float shapeVolume = GetNodeVolume(child.GetChild(0).GetChild(0) as Node3D);
            float bloodVolume = totalBloodVolume * Resource.BodyPartMassCoefficients.GetValueOrDefault(translatedName, 0f);
            float bleedRateMult = BodyPartBleedMultiplier.GetValueOrDefault(translatedName, 1f);

            DamageModule dm = new(
                name: bodyPartName,
                parent: child,
                material: "flesh",
                thickness: .5f,
                bloodVolume: bloodVolume,
                currentBleedRate: 0,
                maxBleedRate: bleedRateMult,
                volume: shapeVolume
            );

            DamageModules.Add(dm);
        }

        _materials = GetNode<Node>("/root/Materials");
        _materialData = (Godot.Collections.Dictionary)_materials.Get("material_data");
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
        for (int i = 0; i < DamageModules.Count; i++)
        {
            DamageModule limb = DamageModules[i];
            float bleedMultiplier = BodyPartBleedMultiplier.GetValueOrDefault(limb.Name, 1);
            float lostBlood = limb.BleedRate * bleedMultiplier * (float)delta;
            limb.BloodVolume = Mathf.Clamp(limb.BloodVolume - lostBlood, 0, limb.MaxBloodVolume);
            DamageModules[i] = limb;

            if (!limb.Name.Equals("Neck")) CheckForEffect(DamageModules[i]);
        }
    }

    private int GetSeverityIndex(DamageModule damageModule)
    {
        float remainingBloodFactor = damageModule.BloodVolume / damageModule.MaxBloodVolume;
        return SeverityThresholds.FindLastIndex(x => x >= remainingBloodFactor);
    }

    private string GetSeverityName(DamageModule damageModule)
    {
        int level = GetSeverityIndex(damageModule);

        KeyValuePair<string, float> severityAtLevel = Severities.ElementAt(level);
        return severityAtLevel.Key;
    }

    private float GetSeverityMultiplier(DamageModule damageModule)
    {
        int level = GetSeverityIndex(damageModule);

        KeyValuePair<string, float> severityAtLevel = Severities.ElementAt(level);
        return severityAtLevel.Value;
    }

    private void CheckForEffect(in DamageModule damageModule)
    {
        float remainingBloodFactor = damageModule.BloodVolume / damageModule.MaxBloodVolume;

        int level = SeverityThresholds.FindLastIndex(x => x >= remainingBloodFactor);

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

    private void UpdateInfo(HitInfo hitInfo)
    {
        _lastHitInfo = hitInfo;
        var hitParent = _lastHitInfo.HitNode.GetParent();
        string hitName = hitParent.Name.ToString();
        _lastHitLimb = DamageModules[FindLimbIndex(hitName)];
    }

    public void Hit(HitInfo hitInfo)
    {
        UpdateInfo(hitInfo);

        (float impactDepth, float kineticEnergy) = GetHitEffects();

        // yo im pretty sure i'm just making up some straight nonsense but i'll fix it later

        float cutTime = impactDepth / _lastHitInfo.WeaponVelocity.Length();
        float cutArea = cutTime * _lastHitInfo.WeaponVelocity.Length() * impactDepth;
        float cutVolume = cutArea * cutTime * impactDepth;
        float bloodLossFactor = cutVolume / _lastHitLimb.Volume;

        GD.Print($"\nCut area: {cutArea} m^2\nCut volume: {cutVolume} m^3\nBlood loss factor: {bloodLossFactor * 100}%");

        float immediateBloodLoss = _lastHitLimb.MaxBloodVolume * (bloodLossFactor + GetImpactDepth());

        _lastHitLimb.BloodVolume -= immediateBloodLoss;

        _lastHitLimb.BleedRate += _lastHitLimb.MaxBloodVolume * bloodLossFactor;

        DamageModules[FindLimbIndex(_lastHitLimb.Name)] = _lastHitLimb;

        // Skeleton.Ragdoll();
        // HumanoidModel humanoid = (HumanoidModel)Skeleton.GetParent().GetParent();
        // humanoid.SwitchTo("dead");
    }

    private (float, float) GetHitEffects()
    {
        return (
            GetImpactDepth(),
            GetKineticEnergyTransfer()
        );
    }

    private float GetKineticEnergyTransfer()
    {
        Vector3 hitDirection = (_lastHitInfo.HitPosition - _lastHitInfo.WeaponHitSource).Normalized();
        Weapon weapon = _lastHitInfo.WeaponNode;

        float impactAngle = GetImpactAngleRadians(hitDirection, _lastHitInfo.HitNormal);

        float weaponSharpnessDivisor = weapon.Sharpness * 1.2f + 1;

        float effectiveEnergyAbsorption = GetEffectiveEnergyAbsorption(
            GetEnergyAbsorption(_lastHitLimb.Material),
            GetImpactDepth(),
            GetThicknessInLineOfSight(impactAngle, _lastHitLimb.Thickness)
        );

        float inflictedKineticEnergy = GetInflictedKineticEnergy(
            weapon.Mass,
            GetVelocityAtImpactAngle(_lastHitInfo.WeaponVelocity.Length(), impactAngle),
            weaponSharpnessDivisor,
            effectiveEnergyAbsorption
        );

        // if (effectiveEnergyAbsorption < 0)
        // {
        //     inflictedKineticEnergy /= 50;
        // }

        return inflictedKineticEnergy;
    }

    private float GetImpactDepth()
    {
        float impactDepth = GetPeriforation(
            _lastHitInfo.EffectiveWeaponLength,
            GetDensity(_lastHitInfo.WeaponNode.Material),
            GetDensity(_lastHitLimb.Material, _lastHitLimb.Thickness)
        );

        return impactDepth;
    }

    private float GetImpactDepthRatio()
    {
        Vector3 hitDirection = (_lastHitInfo.HitPosition - _lastHitInfo.WeaponHitSource).Normalized();
        float depth = GetImpactDepth();
        float thickness = GetThicknessInLineOfSight(
            GetImpactAngleRadians(hitDirection, _lastHitInfo.HitNormal),
            _lastHitLimb.Thickness
        );

        return depth / thickness;
    }

    private static float GetEffectiveEnergyAbsorption(float absorption, float impactDepth, float hitThicknessLos)
    {
        return absorption - (impactDepth / hitThicknessLos);
    }

    private static float GetInflictedKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor, float energyAbsorption)
    {
        return GetImpactKineticEnergy(workingMass, impactVelocity, sharpnessFactor) * (1 - energyAbsorption);
    }

    private static float GetImpactKineticEnergy(float workingMass, float impactVelocity, float sharpnessFactor)
    {
        return workingMass / 2 * Mathf.Pow(impactVelocity / sharpnessFactor, 2);
    }

    private static float GetVelocityAtImpactAngle(float impactVelocity, float impactAngle)
    {
        return Mathf.Abs(impactVelocity * Mathf.Cos(impactAngle));
    }

    private static float GetPeriforation(float workingLength, float workingDensity, float targetDensity)
    {
        return workingLength * (workingDensity / targetDensity);
    }

    private float GetEnergyAbsorption(string material)
    {
        return (float)((Godot.Collections.Dictionary)_materialData[material])["absorption"];
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
        return (float)((Godot.Collections.Dictionary)_materialData[material])["density"];
    }

    private static float GetThicknessInLineOfSight(float impactAngle, float thickness)
    {
        return Mathf.Abs(thickness / Mathf.Cos(impactAngle));
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])", RegexOptions.Compiled)]
    private static partial Regex PascalCaseSplitRegex();

    private static string TranslateName(string nodeName)
    {
        RemoveLeftRightPrefix(ref nodeName);
        return PascalCaseSplitRegex().Replace(nodeName, " ").ToLower().Trim();
    }

    private static void RemoveLeftRightPrefix(ref string name)
    {
        string toRemove = "Left";
        int index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }

        toRemove = "Right";
        index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }
    }

    public static float GetNodeVolume(Node3D node)
    {
        if (node is CollisionShape3D collisionShape)
        {
            return CollisionShapeVolume(collisionShape);
        }

        if (node is MeshInstance3D meshInstance && meshInstance.Mesh is ArrayMesh arrayMesh)
        {
            float volume = 0f;

            for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++)
            {
                var arrays = arrayMesh.SurfaceGetArrays(i);
                var vertices = (Vector3[])arrays[(int)ArrayMesh.ArrayType.Vertex];
                var indices = (int[])arrays[(int)ArrayMesh.ArrayType.Index];

                for (int j = 0; j < indices.Length; j += 3)
                {
                    Vector3 v0 = vertices[indices[j]];
                    Vector3 v1 = vertices[indices[j + 1]];
                    Vector3 v2 = vertices[indices[j + 2]];

                    volume += Math.Abs(v0.Dot(v1.Cross(v2))) / 6f;
                }
            }

            return volume;
        }

        return 0f;
    }

    private static float CollisionShapeVolume(CollisionShape3D collisionShape)
    {
        Shape3D shape = collisionShape.Shape;

        if (shape is BoxShape3D box)
        {
            Vector3 extents = box.Size;
            return extents.X * extents.Y * extents.Z;
        }
        else if (shape is SphereShape3D sphere)
        {
            float radius = sphere.Radius;
            return 1.333f * Mathf.Pi * Mathf.Pow(radius, 3);
        }
        else if (shape is CapsuleShape3D capsule)
        {
            float radius = capsule.Radius;
            float height = capsule.Height;

            float cylinderVolume = Mathf.Pi * Mathf.Pow(radius, 2) * height;
            float sphereVolume = 1.333f * Mathf.Pi * Mathf.Pow(radius, 3);

            return cylinderVolume + sphereVolume;
        }
        return 0f;
    }
}