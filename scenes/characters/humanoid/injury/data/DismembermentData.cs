using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Dismemberment request data.
/// </summary>
public struct DismembermentRequest
{
    public string LimbName;
    public Vector3 DetachPoint;
    public Vector3 DetachForce;
    public Vector3 DetachTorque;
    public float DetachTime;
    
    public DismembermentRequest(
        string limbName,
        Vector3 detachPoint,
        Vector3 detachForce,
        Vector3 detachTorque)
    {
        LimbName = limbName;
        DetachPoint = detachPoint;
        DetachForce = detachForce;
        DetachTorque = detachTorque;
        DetachTime = Time.GetUnixTimeFromSystem();
    }
}

/// <summary>
/// Dismemberment result data.
/// </summary>
public struct DismembermentResult
{
    public bool Success;
    public string LimbName;
    public RigidBody3D DetachedBody;
    public Vector3 DetachPosition;
    public float DetachTime;
    
    public DismembermentResult(
        bool success,
        string limbName,
        RigidBody3D detachedBody,
        Vector3 detachPosition)
    {
        Success = success;
        LimbName = limbName;
        DetachedBody = detachedBody;
        DetachPosition = detachPosition;
        DetachTime = Time.GetUnixTimeFromSystem();
    }
}

/// <summary>
/// Dismemberment configuration for a limb type.
/// </summary>
public struct DismembermentConfig
{
    public float MinimumCutDepthRatio; // Ratio of limb thickness required to dismember
    public float DetachForceMultiplier;
    public float DetachTorqueMultiplier;
    public float DetachedLimbMass;
    public float DetachedLimbFriction;
    public float DetachedLimbBounce;
    
    public DismembermentConfig(
        float minimumCutDepthRatio = 0.8f,
        float detachForceMultiplier = 1.0f,
        float detachTorqueMultiplier = 1.0f,
        float detachedLimbMass = 1.0f,
        float detachedLimbFriction = 0.5f,
        float detachedLimbBounce = 0.1f)
    {
        MinimumCutDepthRatio = minimumCutDepthRatio;
        DetachForceMultiplier = detachForceMultiplier;
        DetachTorqueMultiplier = detachTorqueMultiplier;
        DetachedLimbMass = detachedLimbMass;
        DetachedLimbFriction = detachedLimbFriction;
        DetachedLimbBounce = detachedLimbBounce;
    }
}

/// <summary>
/// Default dismemberment configurations for different limb types.
/// </summary>
public static class DismembermentConfigs
{
    public static DismembermentConfig GetConfigForLimb(string limbName)
    {
        return limbName.ToLower() switch
        {
            "head" => new DismembermentConfig(0.7f, 1.5f, 2.0f, 4.5f, 0.3f, 0.1f),
            "neck" => new DismembermentConfig(0.6f, 2.0f, 3.0f, 1.5f, 0.3f, 0.1f),
            "hand" => new DismembermentConfig(0.75f, 0.8f, 1.0f, 0.4f, 0.4f, 0.2f),
            "foot" => new DismembermentConfig(0.75f, 0.8f, 1.0f, 1.0f, 0.4f, 0.2f),
            "forearm" => new DismembermentConfig(0.8f, 1.0f, 1.2f, 1.2f, 0.4f, 0.15f),
            "shin" => new DismembermentConfig(0.8f, 1.0f, 1.2f, 2.5f, 0.4f, 0.15f),
            "upper arm" => new DismembermentConfig(0.85f, 1.2f, 1.5f, 2.0f, 0.4f, 0.15f),
            "thigh" => new DismembermentConfig(0.85f, 1.2f, 1.5f, 4.0f, 0.4f, 0.15f),
            _ => new DismembermentConfig(0.9f, 1.0f, 1.0f, 1.0f, 0.5f, 0.1f)
        };
    }
    
    /// <summary>
    /// Determines if a limb can be dismembered based on cut depth.
    /// </summary>
    public static bool CanDismember(string limbName, float cutDepth, float limbThickness)
    {
        DismembermentConfig config = GetConfigForLimb(limbName);
        float depthRatio = cutDepth / limbThickness;
        return depthRatio >= config.MinimumCutDepthRatio;
    }
    
    /// <summary>
    /// Calculates the detach force based on weapon velocity and limb config.
    /// </summary>
    public static Vector3 CalculateDetachForce(
        Vector3 weaponVelocity,
        Vector3 hitNormal,
        DismembermentConfig config)
    {
        float forceMagnitude = weaponVelocity.Length() * config.DetachForceMultiplier;
        return hitNormal * forceMagnitude * -1f; // Force in direction of weapon movement
    }
    
    /// <summary>
    /// Calculates the detach torque based on weapon velocity and limb config.
    /// </summary>
    public static Vector3 CalculateDetachTorque(
        Vector3 weaponVelocity,
        Vector3 hitNormal,
        DismembermentConfig config)
    {
        // Create torque perpendicular to both velocity and normal
        Vector3 torqueDirection = weaponVelocity.Cross(hitNormal).Normalized();
        float torqueMagnitude = weaponVelocity.Length() * config.DetachTorqueMultiplier;
        return torqueDirection * torqueMagnitude;
    }
}

/// <summary>
/// Detached limb state for cleanup tracking.
/// </summary>
public struct DetachedLimbState
{
    public string OriginalLimbName;
    public RigidBody3D PhysicsBody;
    public float DetachTime;
    public float Lifetime; // How long the detached limb should exist
    public bool ShouldCleanup;
    
    public DetachedLimbState(
        string originalLimbName,
        RigidBody3D physicsBody,
        float lifetime = 30f)
    {
        OriginalLimbName = originalLimbName;
        PhysicsBody = physicsBody;
        DetachTime = Time.GetUnixTimeFromSystem();
        Lifetime = lifetime;
        ShouldCleanup = false;
    }
    
    public bool ShouldRemove()
    {
        float age = Time.GetUnixTimeFromSystem() - DetachTime;
        return age >= Lifetime;
    }
}
