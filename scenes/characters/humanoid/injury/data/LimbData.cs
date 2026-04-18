using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Core limb state data using struct for cache efficiency.
/// </summary>
public struct LimbData
{
    public string Name;
    public bool IsAttached;
    public float Thickness;
    public int BoneIndex;
    public Node3D VisualNode;
    public RigidBody3D PhysicsBody; // For detached limbs
    public float DetachTime; // Time since detachment (for cleanup)
    
    public LimbData(
        string name,
        bool isAttached,
        float thickness,
        int boneIndex,
        Node3D visualNode,
        RigidBody3D physicsBody = null)
    {
        Name = name;
        IsAttached = isAttached;
        Thickness = thickness;
        BoneIndex = boneIndex;
        VisualNode = visualNode;
        PhysicsBody = physicsBody;
        DetachTime = 0f;
    }
}

/// <summary>
/// Limb attachment state for quick lookup.
/// </summary>
public enum LimbAttachmentState
{
    Attached,
    Detached,
    Severed
}

/// <summary>
/// Limb type for categorization and effect mapping.
/// </summary>
public enum LimbType
{
    Head,
    Neck,
    Thorax,
    Abdomen,
    Pelvis,
    UpperArm,
    Forearm,
    Hand,
    Thigh,
    Shin,
    Foot
}

/// <summary>
/// Maps limb names to their types for effect calculation.
/// </summary>
public static class LimbTypeMapper
{
    public static LimbType GetLimbType(string limbName)
    {
        return limbName.ToLower() switch
        {
            "head" => LimbType.Head,
            "neck" => LimbType.Neck,
            "thorax" => LimbType.Thorax,
            "abdomen" => LimbType.Abdomen,
            "pelvis" => LimbType.Pelvis,
            var n when n.Contains("upper arm") => LimbType.UpperArm,
            var n when n.Contains("forearm") => LimbType.Forearm,
            var n when n.Contains("hand") => LimbType.Hand,
            var n when n.Contains("thigh") => LimbType.Thigh,
            var n when n.Contains("shin") => LimbType.Shin,
            var n when n.Contains("foot") => LimbType.Foot,
            _ => LimbType.Thorax // Default fallback
        };
    }
}
