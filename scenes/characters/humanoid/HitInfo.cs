using Godot;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;

public readonly struct HitInfo(
    Weapon weaponNode,
    Vector3 weaponNormal,
    Vector3 weaponVelocity,
    Vector3 weaponHitSource,
    Node3D hitNode,
    Vector3 hitPosition,
    Vector3 hitNormal,
    Vector3 hitVelocity)
{
    public Weapon WeaponNode { get; } = weaponNode;
    public Vector3 WeaponNormal { get; } = weaponNormal;
    public Vector3 WeaponVelocity { get; } = weaponVelocity;
    public Vector3 WeaponHitSource { get; } = weaponHitSource;
    public Node3D HitNode { get; } = hitNode;
    public Vector3 HitPosition { get; } = hitPosition;
    public Vector3 HitNormal { get; } = hitNormal;
    /// <summary>
    /// The velocity of the hit object in m/s.
    /// </summary>
    public Vector3 HitVelocity { get; } = hitVelocity;
    /// <summary>
    /// The angle the weapon hit something in radians.
    /// </summary>
    public float HitAngle
    {
        get
        {
            return Mathf.Acos(
                WeaponNormal.Dot(HitNormal)
                / (HitNormal.Length() * WeaponNormal.Length()));
        }
    }
    
    public float EffectiveWeaponLength
    {
        get
        {
            return (WeaponHitSource - HitPosition).Length();
        }
    }
}