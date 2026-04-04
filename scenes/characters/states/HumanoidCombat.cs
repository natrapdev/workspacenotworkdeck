using Godot;
using MyFirst3DGame.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidCombat : Node3D
{
    public HumanoidModel Humanoid { get; set; }
    private PhysicsDirectSpaceState3D _physicsSpaceState;

    public override void _Ready()
    {
        Humanoid = GetParent<HumanoidModel>();
        _physicsSpaceState = GetWorld3D().DirectSpaceState;
    }
    // private readonly Dictionary<string, int> _attackPriorities = new()
    // {
    //     {"prepare", 6},
    //     {"unsheathe1", 3},
    //     {"unsheathe2", 3},
    //     {"attack1", 4},
    //     {"attack2", 5},
    // };

    public InputPackage Contextualize(InputPackage input)
    {
        TranslateInputs(input);
        return input;
    }

    public void TranslateInputs(InputPackage input)
    {
        if (input.CombatActionNames.Count <= 0) return;

        string bestAction = input.CombatActionNames.Dequeue();
        string translatedName;

        if (bestAction.Equals("unsheathe1"))
        {
            translatedName = Humanoid.WeaponInventory.GetWeapon(1).Moves[bestAction];
        }
        else if (Humanoid.CurrentWeapon is not null)
        {
            translatedName = Humanoid.CurrentWeapon.Moves[bestAction];
        }
        else
        {
            return;
        }

        State combatState = Humanoid.StateContainer.GetStateByName(translatedName);

        input.Actions.Enqueue(
            combatState,
            combatState.Priority
        );
    }

    public HitInfo ScanForHitsSlash()
    {
        Weapon currentWeapon = Humanoid.CurrentWeapon;
        int rayAmount = currentWeapon.RaycastAmount;
        Marker3D bladeStart = currentWeapon.BladeStartMarker;
        Marker3D bladeEnd = currentWeapon.BladeEndMarker;
        float increment = (bladeEnd.Position.Y - bladeStart.Position.Y) / rayAmount;
        float rayLength = currentWeapon.BladeWidth;

        for (int i = 0; i < rayAmount; i++)
        {
            Vector3 origin = bladeStart.GlobalTransform.Basis
                             * new Vector3(0, increment * i, rayLength)
                             + bladeStart.GlobalTransform.Origin;

            Vector3 target = bladeStart.GlobalTransform.Basis
                             * new Vector3(0, increment * i, -rayLength * 2)
                             + bladeStart.GlobalTransform.Origin;

            var queryParams = PhysicsRayQueryParameters3D.Create(origin, target);
            queryParams.CollideWithAreas = true;
            queryParams.CollideWithBodies = true;
            queryParams.CollisionMask = 2;
            queryParams.Exclude = [Humanoid.GetParent<CollisionObject3D>().GetRid()];

            var result = _physicsSpaceState.IntersectRay(queryParams);

            if (result.Count > 0)
            {
                Node3D hitNode = (Node3D)(GodotObject)result["collider"];
                Vector3 hitPosition = (Vector3)result["position"];
                Vector3 hitNormal = (Vector3)result["normal"];
                Vector3 weaponVelocity = bladeStart.GlobalPosition - bladeStart.Position;
                Vector3 hitVelocity = hitNode is RigidBody3D rb ? rb.LinearVelocity : Vector3.Zero;

                return new HitInfo
                {
                    WeaponNode = currentWeapon,
                    HitNode = hitNode,
                    HitPosition = hitPosition,
                    HitNormal = hitNormal,
                    WeaponVelocity = weaponVelocity,
                    HitVelocity = hitVelocity
                };
            }
        }

        return new HitInfo();
    }

    public HitInfo ScanForHitsStab()
    {
        Weapon currentWeapon = Humanoid.CurrentWeapon;
        Marker3D bladeStart = currentWeapon.BladeStartMarker;
        Marker3D bladeEnd = currentWeapon.BladeEndMarker;

        Vector3 origin = bladeStart.GlobalPosition;
        Vector3 target = bladeEnd.GlobalPosition;

        var queryParams = PhysicsRayQueryParameters3D.Create(origin, target);
        queryParams.CollideWithAreas = true;
        queryParams.CollideWithBodies = true;
        queryParams.CollisionMask = 2;
        queryParams.Exclude = [Humanoid.GetParent<CollisionObject3D>().GetRid()];

        var result = _physicsSpaceState.IntersectRay(queryParams);

        if (result.Count > 0)
        {
            Node3D hitNode = (Node3D)(GodotObject)result["collider"];
            Vector3 hitPosition = (Vector3)result["position"];
            Vector3 hitNormal = (Vector3)result["normal"];
            Vector3 weaponVelocity = bladeStart.GlobalPosition - bladeStart.Position;
            Vector3 hitVelocity = hitNode is RigidBody3D rb ? rb.LinearVelocity : Vector3.Zero;

            return new HitInfo
            {
                WeaponNode = currentWeapon,
                HitNode = hitNode,
                HitPosition = hitPosition,
                HitNormal = hitNormal,
                WeaponVelocity = weaponVelocity,
                HitVelocity = hitVelocity
            };
        }

        return new HitInfo();
    }
}

public struct HitInfo
{
    public Node3D WeaponNode;
    public Vector3 WeaponVelocity;
    public Node3D HitNode;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
    public Vector3 HitVelocity;

}
