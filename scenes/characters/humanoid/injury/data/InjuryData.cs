using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Individual injury instance data using struct for cache efficiency.
/// </summary>
public struct InjuryData
{
    public string AffectedLimb;
    public float Severity; // 0.0 to 1.0
    public float ImmediateBloodLoss;
    public float BleedRateIncrease;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
    public float CutDepth;
    public float TimeOfInjury;
    
    public InjuryData(
        string affectedLimb,
        float severity,
        float immediateBloodLoss,
        float bleedRateIncrease,
        Vector3 hitPosition,
        Vector3 hitNormal,
        float cutDepth)
    {
        AffectedLimb = affectedLimb;
        Severity = severity;
        ImmediateBloodLoss = immediateBloodLoss;
        BleedRateIncrease = bleedRateIncrease;
        HitPosition = hitPosition;
        HitNormal = hitNormal;
        CutDepth = cutDepth;
        TimeOfInjury = (float)Time.GetUnixTimeFromSystem();
    }
}

/// <summary>
/// Injury severity classification.
/// </summary>
public enum InjurySeverity
{
    Negligible = 0,
    Minimal = 1,
    Moderate = 2,
    Severe = 3,
    Critical = 4,
    Catastrophic = 5
}

/// <summary>
/// Helper methods for injury severity calculations.
/// </summary>
public static class InjurySeverityHelper
{
    public static InjurySeverity GetSeverityFromRatio(float bloodLossRatio)
    {
        return bloodLossRatio switch
        {
            >= 0.8f => InjurySeverity.Catastrophic,
            >= 0.6f => InjurySeverity.Critical,
            >= 0.4f => InjurySeverity.Severe,
            >= 0.2f => InjurySeverity.Moderate,
            >= 0.05f => InjurySeverity.Minimal,
            _ => InjurySeverity.Negligible
        };
    }
    
    public static float GetSeverityMultiplier(InjurySeverity severity)
    {
        return severity switch
        {
            InjurySeverity.Catastrophic => 1.0f,
            InjurySeverity.Critical => 0.8f,
            InjurySeverity.Severe => 0.6f,
            InjurySeverity.Moderate => 0.4f,
            InjurySeverity.Minimal => 0.2f,
            InjurySeverity.Negligible => 0.0f,
            _ => 0.0f
        };
    }
}

/// <summary>
/// Stacked injuries container for a limb.
/// </summary>
public struct LimbInjuries
{
    public const int MaxInjuriesPerLimb = 8;
    public int InjuryCount;
    public InjuryData[] Injuries;
    public float TotalBleedRate;
    public float TotalBloodLoss;
    
    public LimbInjuries()
    {
        InjuryCount = 0;
        Injuries = new InjuryData[MaxInjuriesPerLimb];
        TotalBleedRate = 0f;
        TotalBloodLoss = 0f;
    }
    
    public void AddInjury(InjuryData injury)
    {
        if (InjuryCount < MaxInjuriesPerLimb)
        {
            Injuries[InjuryCount] = injury;
            InjuryCount++;
            TotalBleedRate += injury.BleedRateIncrease;
            TotalBloodLoss += injury.ImmediateBloodLoss;
        }
    }
    
    public void RemoveOldestInjury()
    {
        if (InjuryCount > 0)
        {
            TotalBleedRate -= Injuries[0].BleedRateIncrease;
            TotalBloodLoss -= Injuries[0].ImmediateBloodLoss;
            
            // Shift remaining injuries
            for (int i = 0; i < InjuryCount - 1; i++)
            {
                Injuries[i] = Injuries[i + 1];
            }
            
            InjuryCount--;
        }
    }
    
    public void Clear()
    {
        InjuryCount = 0;
        TotalBleedRate = 0f;
        TotalBloodLoss = 0f;
        Injuries = new InjuryData[MaxInjuriesPerLimb];
    }
}
