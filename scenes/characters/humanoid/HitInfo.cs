using Godot;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;

public struct HitInfo(Weapon weaponNode, Vector3 weaponVelocity, Vector3 weaponHitSource, Node3D hitNode, Vector3 hitPosition, Vector3 hitNormal, Vector3 hitVelocity)
{
    public Weapon WeaponNode { get; set; } = weaponNode;
    public Vector3 WeaponVelocity { get; set; } = weaponVelocity;
    public Vector3 WeaponHitSource { get; set; } = weaponHitSource;
    public Node3D HitNode { get; set; } = hitNode;
    public Vector3 HitPosition { get; set; } = hitPosition;
    public Vector3 HitNormal { get; set; } = hitNormal;
    public Vector3 HitVelocity { get; set; } = hitVelocity;
    public readonly float EffectiveWeaponLength
    {
        get
        {
            return (WeaponHitSource - HitPosition).Length();
        }
    }
}