using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using System;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Monitors and executes death conditions.
/// Triggers ragdoll when death criteria are met.
/// </summary>
public partial class DeathSystem : Node
{
    public enum DeathCondition
    {
        TotalBloodBelow40Percent,
        ThoraxBloodBelow30Percent,
        HeadBloodAt0Percent
    }
    
    private DeathState _deathState;
    private CirculationSystem _circulationSystem;
    private HumanoidSkeleton _skeleton;
    private HumanoidModel _humanoid;
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// Event fired when the character dies.
    /// </summary>
    public event Action<DeathCondition?> OnDeath;
    
    /// <summary>
    /// Gets the current death state.
    /// </summary>
    public DeathState GetDeathState() => _deathState;
    
    /// <summary>
    /// Gets whether the character is dead.
    /// </summary>
    public bool IsDead => _deathState.IsDead;
    
    /// <summary>
    /// Gets the cause of death if dead.
    /// </summary>
    public DeathCondition? CauseOfDeath => _deathState.CauseOfDeath;
    
    /// <summary>
    /// Initializes the death system.
    /// </summary>
    public void Initialize(CirculationSystem circulationSystem, HumanoidSkeleton skeleton, HumanoidModel humanoid)
    {
        _circulationSystem = circulationSystem;
        _skeleton = skeleton;
        _humanoid = humanoid;
        
        _deathState = new DeathState
        {
            IsDead = false,
            CauseOfDeath = null,
            TimeOfDeath = 0f
        };
        
        _isInitialized = true;
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized) return;
        
        // Skip checks if already dead
        if (_deathState.IsDead) return;
        
        // Check death conditions
        CheckDeathConditions();
    }
    
    /// <summary>
    /// Checks all death conditions.
    /// </summary>
    private void CheckDeathConditions()
    {
        // Condition 1: Total blood volume below 40%
        if (CheckTotalBloodCondition())
        {
            Die(DeathCondition.TotalBloodBelow40Percent);
            return;
        }
        
        // Condition 2: Thorax blood volume below 30%
        if (CheckThoraxBloodCondition())
        {
            Die(DeathCondition.ThoraxBloodBelow30Percent);
            return;
        }
        
        // Condition 3: Head blood volume at 0%
        if (CheckHeadBloodCondition())
        {
            Die(DeathCondition.HeadBloodAt0Percent);
            return;
        }
    }
    
    /// <summary>
    /// Checks if total blood volume is below 40%.
    /// </summary>
    private bool CheckTotalBloodCondition()
    {
        float bloodRatio = _circulationSystem.BloodVolumeRatio;
        return bloodRatio < 0.4f;
    }
    
    /// <summary>
    /// Checks if thorax blood volume is below 30%.
    /// </summary>
    private bool CheckThoraxBloodCondition()
    {
        float thoraxRatio = _circulationSystem.GetLimbBloodRatio("thorax");
        return thoraxRatio < 0.3f;
    }
    
    /// <summary>
    /// Checks if head blood volume is at 0%.
    /// </summary>
    private bool CheckHeadBloodCondition()
    {
        float headRatio = _circulationSystem.GetLimbBloodRatio("head");
        return headRatio <= 0f;
    }
    
    /// <summary>
    /// Triggers death.
    /// </summary>
    private void Die(DeathCondition cause)
    {
        if (_deathState.IsDead) return;
        
        // Update death state
        _deathState.IsDead = true;
        _deathState.CauseOfDeath = cause;
        _deathState.TimeOfDeath = Time.GetUnixTimeFromSystem();
        
        // Trigger ragdoll
        if (_skeleton != null)
        {
            _skeleton.Ragdoll();
        }
        
        // Switch to dead state if humanoid is available
        if (_humanoid != null)
        {
            _humanoid.SwitchTo("dead");
        }
        
        // Fire death event
        OnDeath?.Invoke(cause);
        
        GD.Print($"Character died from: {cause}");
    }
    
    /// <summary>
    /// Forces death without checking conditions.
    /// Useful for instant death effects.
    /// </summary>
    public void ForceDeath(DeathCondition cause)
    {
        if (_deathState.IsDead) return;
        
        Die(cause);
    }
    
    /// <summary>
    /// Resets the death state (for respawning).
    /// </summary>
    public void Reset()
    {
        _deathState = new DeathState
        {
            IsDead = false,
            CauseOfDeath = null,
            TimeOfDeath = 0f
        };
    }
    
    /// <summary>
    /// Gets a human-readable description of the death cause.
    /// </summary>
    public static string GetDeathCauseDescription(DeathCondition cause)
    {
        return cause switch
        {
            DeathCondition.TotalBloodBelow40Percent => "Exsanguination",
            DeathCondition.ThoraxBloodBelow30Percent => "Chest trauma",
            DeathCondition.HeadBloodAt0Percent => "Decapitation",
            _ => "Unknown cause"
        };
    }
}

/// <summary>
/// Death state data structure.
/// </summary>
public struct DeathState
{
    public bool IsDead;
    public DeathCondition? CauseOfDeath;
    public float TimeOfDeath;
    
    public DeathState()
    {
        IsDead = false;
        CauseOfDeath = null;
        TimeOfDeath = 0f;
    }
}
