using Godot;
using MyFirst3DGame.scenes.characters.states;
using System.Threading;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Damage request for asynchronous damage processing.
/// </summary>
public struct DamageRequest
{
    public HitInfo HitInfo;
    public DamageModule TargetLimb;
    public int LimbIndex;
    public CancellationToken Token;
    public float RequestTime;
    
    public DamageRequest(
        HitInfo hitInfo,
        DamageModule targetLimb,
        int limbIndex,
        CancellationToken token)
    {
        HitInfo = hitInfo;
        TargetLimb = targetLimb;
        LimbIndex = limbIndex;
        Token = token;
        RequestTime = Time.GetUnixTimeFromSystem();
    }
}

/// <summary>
/// Damage result from asynchronous damage processing.
/// </summary>
public struct DamageResult
{
    public bool Success;
    public string LimbName;
    public float ImmediateBloodLoss;
    public float BleedRateIncrease;
    public bool ShouldDismember;
    public float CutDepth;
    public float LimbThickness;
    public float Severity;
    public InjurySeverity SeverityLevel;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
    public float ProcessingTime;
    
    public DamageResult(
        bool success,
        string limbName,
        float immediateBloodLoss,
        float bleedRateIncrease,
        bool shouldDismember,
        float cutDepth,
        float limbThickness,
        float severity,
        InjurySeverity severityLevel,
        Vector3 hitPosition,
        Vector3 hitNormal,
        float processingTime)
    {
        Success = success;
        LimbName = limbName;
        ImmediateBloodLoss = immediateBloodLoss;
        BleedRateIncrease = bleedRateIncrease;
        ShouldDismember = shouldDismember;
        CutDepth = cutDepth;
        LimbThickness = limbThickness;
        Severity = severity;
        SeverityLevel = severityLevel;
        HitPosition = hitPosition;
        HitNormal = hitNormal;
        ProcessingTime = processingTime;
    }
    
    /// <summary>
    /// Creates a failed damage result.
    /// </summary>
    public static DamageResult Failed(string limbName)
    {
        return new DamageResult(
            false,
            limbName,
            0f,
            0f,
            false,
            0f,
            0f,
            0f,
            InjurySeverity.Negligible,
            Vector3.Zero,
            Vector3.Zero,
            0f
        );
    }
}

/// <summary>
/// Animation control result for the combat system.
/// Determines whether the attack animation should continue or stop.
/// </summary>
public enum AnimationControlResult
{
    Continue, // Continue the attack animation (dismemberment or successful hit)
    Stop,     // Stop the attack animation (hit but not enough to dismember)
    Ignore    // Ignore the result (no hit or invalid target)
}

/// <summary>
/// Combined damage processing result including animation control.
/// </summary>
public struct DamageProcessingResult
{
    public DamageResult DamageResult;
    public AnimationControlResult AnimationControl;
    
    public DamageProcessingResult(DamageResult damageResult, AnimationControlResult animationControl)
    {
        DamageResult = damageResult;
        AnimationControl = animationControl;
    }
}
