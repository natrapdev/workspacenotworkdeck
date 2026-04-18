using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;

/// <summary>
/// Interface for entities that can receive damage.
/// Allows for different damage handling implementations.
/// </summary>
public interface IDamageReceiver
{
    /// <summary>
    /// Receives damage asynchronously.
    /// </summary>
    /// <param name="request">The damage request containing hit information</param>
    /// <returns>Task representing the async damage processing</returns>
    Task ReceiveDamageAsync(DamageRequest request);
    
    /// <summary>
    /// Determines if the entity can currently receive damage.
    /// </summary>
    /// <returns>True if damage can be received, false otherwise</returns>
    bool CanReceiveDamage();
    
    /// <summary>
    /// Gets the current health/blood state of the entity.
    /// </summary>
    /// <returns>The current health state</returns>
    HealthState GetHealthState();
}

/// <summary>
/// Health state data structure.
/// </summary>
public struct HealthState
{
    public float TotalBloodVolume;
    public float CurrentBloodVolume;
    public float BloodVolumeRatio;
    public float ThoraxBloodVolume;
    public float ThoraxBloodRatio;
    public float HeadBloodVolume;
    public float HeadBloodRatio;
    public bool IsDead;
    
    public HealthState(
        float totalBloodVolume,
        float currentBloodVolume,
        float thoraxBloodVolume,
        float headBloodVolume)
    {
        TotalBloodVolume = totalBloodVolume;
        CurrentBloodVolume = currentBloodVolume;
        BloodVolumeRatio = totalBloodVolume > 0 ? currentBloodVolume / totalBloodVolume : 0f;
        ThoraxBloodVolume = thoraxBloodVolume;
        ThoraxBloodRatio = thoraxBloodVolume > 0 ? thoraxBloodVolume / (totalBloodVolume * 0.2f) : 0f;
        HeadBloodVolume = headBloodVolume;
        HeadBloodRatio = headBloodVolume > 0 ? headBloodVolume / (totalBloodVolume * 0.05f) : 0f;
        IsDead = false;
    }
}
