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
        var parentBody = Humanoid.GetParent<CollisionObject3D>();

        if (parentBody?.Owner is null)
        {
            GD.PrintErr("Could not find parent CharacterBody3D");
            return;
        }
        
        var damageModel = parentBody.GetNodeOrNull<DamageModel>("DamageModel");
        
        if (damageModel is null)
        {
            GD.PrintErr($"Could not find DamageModel in {parentBody.Name}");
            return;
        }
        
        _exclusionList.Add(parentBody.GetRid());
        
        // Find and exclude all Area3D nodes in the DamageModel
        CollectArea3DRids(damageModel, _exclusionList);
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
    
    private static Array<Rid> BuildExclusionListButAllowOneNode(Node toCollideWith, HitInfo hitInfo)
    {
        var rids = new Array<Rid>();
        
        var parentBody = ((Limb)hitInfo.HitNode.GetParent()).Model.Humanoid.GetParent<CollisionObject3D>();
        
        if (parentBody?.Owner is null)
        {
            GD.PrintErr("Could not find parent CharacterBody3D");
            return rids;
        }
        
        var damageModel = parentBody.GetNodeOrNull<DamageModel>("DamageModel");
        
        if (damageModel is null)
        {
            GD.PrintErr($"Could not find DamageModel in {parentBody.Name}");
            return rids;
        }

        CollectArea3DRidsWithExceptions(damageModel, rids, toCollideWith);

        return rids;
    }
    
    private static void CollectArea3DRidsWithExceptions(Node node, Array<Rid> exclusionList, Node toCollideWith)
    {
        if (node is Area3D area)
        {
            exclusionList.Add(area.GetRid());
        }

        foreach (Node child in node.GetChildren())
        {
            if (child == toCollideWith)
            {
                continue;
            }
            CollectArea3DRidsWithExceptions(child, exclusionList, toCollideWith);
        }
    }

    public InputPackage Contextualize(InputPackage input)
    {
        TranslateInputs(input);
        return input;
    }

    private void TranslateInputs(InputPackage input)
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

    public HitInfo ScanForHitsSlash(Weapon weapon)
    {
        if (weapon == null) return new HitInfo();

        int rayAmount = weapon.RaycastAmount;
        Marker3D bladeStart = weapon.BladeStartMarker;
        Marker3D bladeEnd = weapon.BladeEndMarker;

        // Pre-calculate values outside the loop
        float increment = (bladeEnd.Position.Y - bladeStart.Position.Y) / rayAmount;
        float rayLength = weapon.BladeWidth / 2;
        Basis bladeBasis = bladeEnd.GlobalTransform.Basis;
        Vector3 bladeOrigin = bladeEnd.GlobalTransform.Origin;
        Vector3 forwardVector = bladeBasis.Z.Normalized();
        Vector3 weaponVelocity = 25f * forwardVector;

        // Reuse query parameters where possible
        var queryParams = PhysicsRayQueryParameters3D.Create(Vector3.Zero, Vector3.Zero);
        queryParams.CollideWithAreas = true;
        queryParams.CollisionMask = 2;
        queryParams.Exclude = _exclusionList;

        for (int i = 0; i < rayAmount; i++)
        {
            float yOffset = -increment * i;

            // Calculate origin and target using pre-calculated basis and origin
            Vector3 origin = bladeBasis * new Vector3(rayLength, yOffset, 0) + bladeOrigin;
            Vector3 target = bladeBasis * new Vector3(-rayLength, yOffset, 0) + bladeOrigin;
            
            // CreateDebugBlock(origin, weapon);
            // CreateDebugSphere(target, weapon);

            // Update query parameters
            queryParams.From = origin;
            queryParams.To = target;

            Dictionary result = _physicsSpaceState.IntersectRay(queryParams);

            if (result.Count > 0)
            {
                float workingLength = Mathf.Max(Mathf.Abs(yOffset), .02f);
                HitInfo hi = CollectHitInformation(result, weapon, weaponVelocity, forwardVector, weapon.BladeWidth, workingLength);
                // HandleHit(hi);
                return hi;
            }
        }

        return new HitInfo();
    }

    public HitInfo ScanForHitsStab(Weapon weapon)
    {
        if (weapon == null) return new HitInfo();

        Marker3D bladeStart = weapon.BladeStartMarker;
        Marker3D bladeEnd = weapon.BladeEndMarker;

        Vector3 origin = bladeStart.GlobalPosition;
        Vector3 target = bladeEnd.GlobalPosition;
        Vector3 forwardVector = -origin.DirectionTo(target);
        Vector3 weaponVelocity = 25f * forwardVector;

        var queryParams = PhysicsRayQueryParameters3D.Create(origin, target);
        queryParams.CollideWithAreas = true;
        queryParams.CollisionMask = 2;
        queryParams.Exclude = _exclusionList;

        Dictionary result = _physicsSpaceState.IntersectRay(queryParams);

        if (result.Count > 0)
        {
            HitInfo hi = CollectHitInformation(result, weapon, weaponVelocity, forwardVector, weapon.BladeLength, weapon.BladeWidth);
            // HandleHit(hi);
            return hi;
        }

        return new HitInfo();
    }

    private static HitInfo CollectHitInformation(
        Dictionary result,
        Weapon currentWeapon,
        Vector3 weaponVelocity,
        Vector3 weaponNormal,
        float workingLength,
        float workingWidth)
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
            weaponNormal,
            weaponVelocity,
            workingLength,
            workingWidth,
            hitNode,
            hitPosition,
            hitNormal,
            hitVelocity
        );
    }

    public float GetEffectiveThicknessOfHit(HitInfo hitInfo)
    {
        const int maxAttempts = 8;

        Vector3 rayDirection = -hitInfo.WeaponNormal;
        Vector3 rayOrigin = hitInfo.HitPosition + rayDirection * 0.001f;

        // CreateDebugSphere(rayOrigin, hitInfo.HitNode);

        var query = PhysicsRayQueryParameters3D.Create(rayOrigin + rayDirection * 5f, rayOrigin);
        query.CollisionMask = 2;
        query.Exclude = BuildExclusionListButAllowOneNode(hitInfo.HitNode, hitInfo);
        query.CollideWithBodies = false;
        query.HitBackFaces = true;
        query.HitFromInside = true;
        query.CollideWithAreas = true;
        
        // CreateDebugBlock(rayOrigin + rayDirection * 5f, hitInfo.HitNode);
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Dictionary result = _physicsSpaceState.IntersectRay(query);
            
            if (result.Count <= 0) return 999;
            if ((Node3D)result["collider"] != hitInfo.HitNode)
            {
                var collider = (Area3D)result["collider"];
                query.Exclude = query.GetExclude() + [collider.GetRid()];
                continue;
            }
            
            var exitPoint = (Vector3)result["position"];
            // CreateDebugBlock(exitPoint, hitInfo.HitNode); 
            return (exitPoint - hitInfo.HitPosition).Length();
        }

        return 999;
    }

    private static void HandleHit(HitInfo hitInfo)
    {
        Node hit = hitInfo.HitNode.GetParent();
        
        switch (hit)
        {
            case Limb limb:
            {
                limb.Hit(hitInfo);
                break;
            }
            default: return;
        }
    }
    
    private static void CreateDebugBlock(Vector3 pos, Node parent) {
        MeshInstance3D hit = new();
        parent.AddChild(hit);
        hit.Mesh = new BoxMesh();
        hit.Scale = new Vector3(.05f, .05f, .05f);
        hit.GlobalPosition = pos;
    }
    private static void CreateDebugSphere(Vector3 pos, Node parent) {
        MeshInstance3D hit = new();
        parent.AddChild(hit);
        hit.Mesh = new SphereMesh();
        hit.Scale = new Vector3(.05f, .05f, .05f);
        hit.GlobalPosition = pos;
    }
}
