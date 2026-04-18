using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.examples;

/// <summary>
/// Example custom injury effect modifier.
/// This demonstrates how to create custom modifiers for the injury system.
/// </summary>
public partial class CustomInjuryEffectModifier : Node, IInjuryEffectModifier
{
    /// <summary>
    /// Priority determines the order in which modifiers are applied.
    /// Higher priority modifiers are applied later.
    /// </summary>
    public float Priority => 10.0f;
    
    /// <summary>
    /// Determines if this modifier should be applied for the given injury.
    /// </summary>
    public bool ShouldApply(InjuryData injury, LimbType limbType)
    {
        // Only apply to severe or critical injuries
        if (injury.Severity < 0.6f) return false;
        
        // Only apply to specific limb types
        return limbType == LimbType.Head || 
               limbType == LimbType.Thorax || 
               limbType == LimbType.Neck;
    }
    
    /// <summary>
    /// Applies the modifier to the character stat modifiers.
    /// </summary>
    public void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType)
    {
        // Apply different effects based on limb type
        switch (limbType)
        {
            case LimbType.Head:
                // Head injuries cause severe accuracy and reaction penalties
                modifiers.AttackSpeedMultiplier *= 0.5f;
                modifiers.DodgeSpeedMultiplier *= 0.6f;
                break;
                
            case LimbType.Neck:
                // Neck injuries cause severe stamina penalties
                modifiers.StaminaRegenMultiplier *= 0.3f;
                modifiers.MaxStaminaMultiplier *= 0.5f;
                break;
                
            case LimbType.Thorax:
                // Thorax injuries cause general combat penalties
                modifiers.AttackStrengthMultiplier *= 0.7f;
                modifiers.BlockStrengthMultiplier *= 0.6f;
                break;
        }
        
        // Apply additional penalty based on injury severity
        float severityPenalty = 1.0f - (injury.Severity * 0.3f);
        modifiers.WalkSpeedMultiplier *= severityPenalty;
        modifiers.AttackSpeedMultiplier *= severityPenalty;
    }
}

/// <summary>
/// Example: Bleeding effect modifier.
/// Adds additional penalties for bleeding injuries.
/// </summary>
public partial class BleedingEffectModifier : Node, IInjuryEffectModifier
{
    public float Priority => 5.0f;
    
    public bool ShouldApply(InjuryData injury, LimbType limbType)
    {
        // Apply to any injury with significant bleed rate
        return injury.BleedRateIncrease > 0.1f;
    }
    
    public void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType)
    {
        // Bleeding causes progressive stamina drain
        float bleedPenalty = Mathf.Clamp(1.0f - (injury.BleedRateIncrease * 2.0f), 0.5f, 1.0f);
        
        modifiers.StaminaRegenMultiplier *= bleedPenalty;
        modifiers.MaxStaminaMultiplier *= bleedPenalty;
    }
}

/// <summary>
/// Example: Pain effect modifier.
/// Adds penalties based on pain from injuries.
/// </summary>
public partial class PainEffectModifier : Node, IInjuryEffectModifier
{
    public float Priority => 15.0f; // Applied after other modifiers
    
    public bool ShouldApply(InjuryData injury, LimbType limbType)
    {
        // Apply to moderate or worse injuries
        return injury.Severity >= 0.4f;
    }
    
    public void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType)
    {
        // Pain causes reaction time penalties
        float painLevel = injury.Severity;
        
        // Reduce attack speed due to pain
        modifiers.AttackSpeedMultiplier *= (1.0f - (painLevel * 0.2f));
        
        // Reduce dodge speed due to pain
        modifiers.DodgeSpeedMultiplier *= (1.0f - (painLevel * 0.3f));
        
        // Pain affects block ability
        modifiers.BlockStrengthMultiplier *= (1.0f - (painLevel * 0.25f));
    }
}

/// <summary>
/// Example: Adrenaline effect modifier.
/// Reduces penalties temporarily after taking damage.
/// </summary>
public partial class AdrenalineEffectModifier : Node, IInjuryEffectModifier
{
    private float _adrenalineLevel = 0f;
    private float _adrenalineDecayRate = 0.5f; // Per second
    
    public float Priority => 20.0f; // Applied last
    
    public bool ShouldApply(InjuryData injury, LimbType limbType)
    {
        // Apply adrenaline when taking significant damage
        if (injury.Severity >= 0.5f)
        {
            // Boost adrenaline level
            _adrenalineLevel = Mathf.Min(_adrenalineLevel + 0.3f, 1.0f);
        }
        
        return _adrenalineLevel > 0.1f;
    }
    
    public void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType)
    {
        // Adrenaline temporarily boosts some stats
        float adrenalineBonus = 1.0f + (_adrenalineLevel * 0.3f);
        
        modifiers.AttackSpeedMultiplier *= adrenalineBonus;
        modifiers.WalkSpeedMultiplier *= adrenalineBonus;
        modifiers.DodgeSpeedMultiplier *= adrenalineBonus;
        
        // But reduces precision (block strength)
        modifiers.BlockStrengthMultiplier *= (1.0f - (_adrenalineLevel * 0.2f));
    }
    
    public override void _Process(double delta)
    {
        // Decay adrenaline over time
        _adrenalineLevel = Mathf.Max(0f, _adrenalineLevel - (_adrenalineDecayRate * (float)delta));
    }
}
