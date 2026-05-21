using Godot;
using MyFirst3DGame.Items;
using MyFirst3DGame.scenes.characters.humanoid;
using System.Threading.Tasks;
using Godot.Collections;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidCombat : Node3D
{
    public HumanoidModel Humanoid { get; private set; }
    private PhysicsDirectSpaceState3D _physicsSpaceState;

    private Array<Rid> _exclusionList;
    private Array<Rid> _tempExclusionList;

    public override void _Ready()
    {
        Humanoid = GetParent<HumanoidModel>();
        _physicsSpaceState = GetWorld3D().DirectSpaceState;
        BuildExclusionList();
    }

    private void BuildExclusionList()
    {
        _exclusionList = [];

        // Exclude the parent CharacterBody3D
        CollisionObject3D parentBody = Humanoid.GetParent<CollisionObject3D>();

        if (parentBody is not null)
        {
            _exclusionList.Add(parentBody.GetRid());
        }

        // Find and exclude all Area3D nodes in the DamageModel
        DamageModel damageModel = Humanoid.GetNodeOrNull<DamageModel>("DamageModel");
        if (damageModel != null)
        {
            CollectArea3DRids(damageModel, _exclusionList);
        }
    }

    private static void CollectArea3DRids(Node node, Array<Rid> exclusionList)
    {
        if (node is Area3D area)
        {
            exclusionList.Add(area.GetRid());
        }

        foreach (Node child in node.GetChildren())
        {
            CollectArea3DRids(child, exclusionList);
        }
    }

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

        if (bestAction.Equals("unsheathe1") && Humanoid.WeaponInventory.GetWeapon(1) is not null)
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
        if (currentWeapon == null) return new HitInfo();

        int rayAmount = currentWeapon.RaycastAmount;
        Marker3D bladeStart = currentWeapon.BladeStartMarker;
        Marker3D bladeEnd = currentWeapon.BladeEndMarker;

        // Pre-calculate values outside the loop
        float increment = (bladeEnd.Position.Y - bladeStart.Position.Y) / rayAmount;
        float rayLength = currentWeapon.BladeWidth;
        Basis bladeBasis = bladeStart.GlobalTransform.Basis;
        Vector3 bladeOrigin = bladeStart.GlobalTransform.Origin;
        Vector3 forwardVector = bladeBasis.Z.Normalized();
        Vector3 weaponVelocity = 25f * forwardVector;

        // Reuse query parameters where possible
        var queryParams = PhysicsRayQueryParameters3D.Create(Vector3.Zero, Vector3.Zero);
        queryParams.CollideWithAreas = true;
        queryParams.CollisionMask = 2;
        queryParams.Exclude = _exclusionList;

        for (int i = 0; i < rayAmount; i++)
        {
            float yOffset = increment * i;

            // Calculate origin and target using pre-calculated basis and origin
            Vector3 origin = bladeBasis * new Vector3(rayLength, yOffset, 0) + bladeOrigin;
            Vector3 target = bladeBasis * new Vector3(-rayLength, yOffset, 0) + bladeOrigin;

            // Update query parameters
            queryParams.From = origin;
            queryParams.To = target;

            Dictionary result = _physicsSpaceState.IntersectRay(queryParams);

            if (result.Count > 0)
            {
                // Return immediately on first hit
                return CollectHitInformation(result, currentWeapon, origin, weaponVelocity);
            }
        }

        return new HitInfo();
    }

    public HitInfo ScanForHitsStab()
    {
        Weapon currentWeapon = Humanoid.CurrentWeapon;
        if (currentWeapon == null) return new HitInfo();

        Marker3D bladeStart = currentWeapon.BladeStartMarker;
        Marker3D bladeEnd = currentWeapon.BladeEndMarker;

        Vector3 origin = bladeStart.GlobalPosition;
        Vector3 target = bladeEnd.GlobalPosition;
        Vector3 forwardVector = bladeStart.GlobalTransform.Basis.Z.Normalized();
        Vector3 weaponVelocity = 25f * forwardVector;

        var queryParams = PhysicsRayQueryParameters3D.Create(origin, target);
        queryParams.CollideWithAreas = true;
        queryParams.CollisionMask = 2;
        queryParams.Exclude = _exclusionList;

        Dictionary result = _physicsSpaceState.IntersectRay(queryParams);

        return CollectHitInformation(result, currentWeapon, origin, weaponVelocity);
    }

    private static HitInfo CollectHitInformation(
        Godot.Collections.Dictionary result,
        Weapon currentWeapon,
        Vector3 weaponHitSource,
        Vector3 weaponVelocity)
    {
        if (result.Count <= 0) return new HitInfo();

        var hitNode = (Node3D)(GodotObject)result["collider"];
        var hitPosition = (Vector3)result["position"];
        var hitNormal = (Vector3)result["normal"];
        //TODO: include velocity of hit targets.
        Vector3 hitVelocity = hitNode is RigidBody3D rb ? rb.LinearVelocity : Vector3.Zero;

        // MeshInstance3D hit = new();
        // hitNode.AddChild(hit);
        // hit.Mesh = new BoxMesh();
        // hit.Scale = new Vector3(.1f, .1f, .1f);
        // hit.GlobalPosition = hitPosition;

        return new HitInfo(
            currentWeapon,
            weaponVelocity,
            weaponHitSource,
            hitNode,
            hitPosition,
            hitNormal,
            hitVelocity
        );
    }
}
