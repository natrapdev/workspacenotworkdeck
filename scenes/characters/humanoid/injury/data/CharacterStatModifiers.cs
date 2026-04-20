using System;
using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.data;

/// <summary>
/// Stat types that can be affected by injuries.
/// </summary>
public enum StatType
{
    WalkSpeed,
    AttackSpeed,
    AttackStrength,
    StaminaRegen,
    MaxStamina,
    DodgeSpeed,
    BlockStrength
}

/// <summary>
/// Character stat modifiers based on injuries.
/// All values are multipliers (1.0 = no change, 0.8 = 20% reduction, 1.2 = 20% increase).
/// </summary>
public struct CharacterStatModifiers
{
    public float WalkSpeedMultiplier;
    public float AttackSpeedMultiplier;
    public float AttackStrengthMultiplier;
    public float StaminaRegenMultiplier;
    public float MaxStaminaMultiplier;
    public float DodgeSpeedMultiplier;
    public float BlockStrengthMultiplier;
    
    public CharacterStatModifiers()
    {
        WalkSpeedMultiplier = 1.0f;
        AttackSpeedMultiplier = 1.0f;
        AttackStrengthMultiplier = 1.0f;
        StaminaRegenMultiplier = 1.0f;
        MaxStaminaMultiplier = 1.0f;
        DodgeSpeedMultiplier = 1.0f;
        BlockStrengthMultiplier = 1.0f;
    }
    
    /// <summary>
    /// Gets the multiplier for a specific stat type.
    /// </summary>
    public float GetMultiplier(StatType statType)
    {
        return statType switch
        {
            StatType.WalkSpeed => WalkSpeedMultiplier,
            StatType.AttackSpeed => AttackSpeedMultiplier,
            StatType.AttackStrength => AttackStrengthMultiplier,
            StatType.StaminaRegen => StaminaRegenMultiplier,
            StatType.MaxStamina => MaxStaminaMultiplier,
            StatType.DodgeSpeed => DodgeSpeedMultiplier,
            StatType.BlockStrength => BlockStrengthMultiplier,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Sets the multiplier for a specific stat type.
    /// </summary>
    public void SetMultiplier(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.WalkSpeed:
                WalkSpeedMultiplier = value;
                break;
            case StatType.AttackSpeed:
                AttackSpeedMultiplier = value;
                break;
            case StatType.AttackStrength:
                AttackStrengthMultiplier = value;
                break;
            case StatType.StaminaRegen:
                StaminaRegenMultiplier = value;
                break;
            case StatType.MaxStamina:
                MaxStaminaMultiplier = value;
                break;
            case StatType.DodgeSpeed:
                DodgeSpeedMultiplier = value;
                break;
            case StatType.BlockStrength:
                BlockStrengthMultiplier = value;
                break;
        }
    }
    
    /// <summary>
    /// Applies a modifier to a specific stat type.
    /// The modifier is multiplied with the current value.
    /// </summary>
    public void ApplyModifier(StatType statType, float modifier)
    {
        float current = GetMultiplier(statType);
        SetMultiplier(statType, current * modifier);
    }
    
    /// <summary>
    /// Resets all modifiers to their default values (1.0).
    /// </summary>
    public void Reset()
    {
        WalkSpeedMultiplier = 1.0f;
        AttackSpeedMultiplier = 1.0f;
        AttackStrengthMultiplier = 1.0f;
        StaminaRegenMultiplier = 1.0f;
        MaxStaminaMultiplier = 1.0f;
        DodgeSpeedMultiplier = 1.0f;
        BlockStrengthMultiplier = 1.0f;
    }
    
    /// <summary>
    /// Clamps all multipliers to a reasonable range.
    /// </summary>
    public void Clamp(float min = 0.1f, float max = 2.0f)
    {
        WalkSpeedMultiplier = Math.Clamp(WalkSpeedMultiplier, min, max);
        AttackSpeedMultiplier = Mathf.Clamp(AttackSpeedMultiplier, min, max);
        AttackStrengthMultiplier = Mathf.Clamp(AttackStrengthMultiplier, min, max);
        StaminaRegenMultiplier = Mathf.Clamp(StaminaRegenMultiplier, min, max);
        MaxStaminaMultiplier = Mathf.Clamp(MaxStaminaMultiplier, min, max);
        DodgeSpeedMultiplier = Mathf.Clamp(DodgeSpeedMultiplier, min, max);
        BlockStrengthMultiplier = Mathf.Clamp(BlockStrengthMultiplier, min, max);
    }
}

/// <summary>
/// Default stat modifier configurations for different injury severities.
/// </summary>
public static class StatModifierPresets
{
    /// <summary>
    /// Gets stat modifiers based on injury severity.
    /// </summary>
    public static CharacterStatModifiers GetModifiersForSeverity(InjurySeverity severity, LimbType limbType)
    {
        CharacterStatModifiers modifiers = new CharacterStatModifiers();
        
        float severityMultiplier = InjurySeverityHelper.GetSeverityMultiplier(severity);
        
        switch (limbType)
        {
            case LimbType.Head:
                modifiers.AttackSpeedMultiplier = 1.0f - (severityMultiplier * 0.3f);
                modifiers.AttackStrengthMultiplier = 1.0f - (severityMultiplier * 0.2f);
                modifiers.MaxStaminaMultiplier = 1.0f - (severityMultiplier * 0.4f);
                break;
                
            case LimbType.Neck:
                modifiers.AttackSpeedMultiplier = 1.0f - (severityMultiplier * 0.5f);
                modifiers.AttackStrengthMultiplier = 1.0f - (severityMultiplier * 0.4f);
                modifiers.StaminaRegenMultiplier = 1.0f - (severityMultiplier * 0.6f);
                break;
                
            case LimbType.Thorax:
                modifiers.AttackStrengthMultiplier = 1.0f - (severityMultiplier * 0.3f);
                modifiers.MaxStaminaMultiplier = 1.0f - (severityMultiplier * 0.5f);
                modifiers.StaminaRegenMultiplier = 1.0f - (severityMultiplier * 0.3f);
                break;
                
            case LimbType.Abdomen:
            case LimbType.Pelvis:
                modifiers.WalkSpeedMultiplier = 1.0f - (severityMultiplier * 0.4f);
                modifiers.DodgeSpeedMultiplier = 1.0f - (severityMultiplier * 0.5f);
                modifiers.MaxStaminaMultiplier = 1.0f - (severityMultiplier * 0.3f);
                break;
                
            case LimbType.UpperArm:
                modifiers.AttackStrengthMultiplier = 1.0f - (severityMultiplier * 0.4f);
                modifiers.BlockStrengthMultiplier = 1.0f - (severityMultiplier * 0.3f);
                break;
                
            case LimbType.Forearm:
                modifiers.AttackSpeedMultiplier = 1.0f - (severityMultiplier * 0.3f);
                modifiers.BlockStrengthMultiplier = 1.0f - (severityMultiplier * 0.4f);
                break;
                
            case LimbType.Hand:
                modifiers.AttackSpeedMultiplier = 1.0f - (severityMultiplier * 0.2f);
                break;
                
            case LimbType.Thigh:
                modifiers.WalkSpeedMultiplier = 1.0f - (severityMultiplier * 0.5f);
                modifiers.DodgeSpeedMultiplier = 1.0f - (severityMultiplier * 0.4f);
                break;
                
            case LimbType.Shin:
                modifiers.WalkSpeedMultiplier = 1.0f - (severityMultiplier * 0.4f);
                break;
                
            case LimbType.Foot:
                modifiers.WalkSpeedMultiplier = 1.0f - (severityMultiplier * 0.2f);
                break;
        }
        
        modifiers.Clamp();
        return modifiers;
    }
}
