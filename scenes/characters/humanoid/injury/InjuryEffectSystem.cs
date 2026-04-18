using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Applies injury effects to character stats.
/// Calculates stat modifiers based on limb injuries.
/// </summary>
public partial class InjuryEffectSystem : Node
{
    private CirculationSystem _circulationSystem;
    private LimbManager _limbManager;
    private HumanoidModel _humanoid;
    
    private readonly List<IInjuryEffectModifier> _effectModifiers = new();
    private CharacterStatModifiers _currentModifiers;
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// Event fired when stat modifiers change.
    /// </summary>
    public event Action<CharacterStatModifiers> OnStatModifiersChanged;
    
    /// <summary>
    /// Gets the current stat modifiers.
    /// </summary>
    public CharacterStatModifiers CurrentModifiers => _currentModifiers;
    
    /// <summary>
    /// Initializes the injury effect system.
    /// </summary>
    public void Initialize(
        CirculationSystem circulationSystem,
        LimbManager limbManager,
        HumanoidModel humanoid)
    {
        _circulationSystem = circulationSystem;
        _limbManager = limbManager;
        _humanoid = humanoid;
        
        _currentModifiers = new CharacterStatModifiers();
        
        // Subscribe to circulation changes
        _circulationSystem.OnBloodVolumeChanged += OnBloodVolumeChanged;
        
        _isInitialized = true;
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized) return;
        
        // Update stat modifiers based on current injuries
        UpdateStatModifiers();
    }
    
    /// <summary>
    /// Registers a custom injury effect modifier.
    /// </summary>
    public void RegisterEffectModifier(IInjuryEffectModifier modifier)
    {
        if (!_effectModifiers.Contains(modifier))
        {
            _effectModifiers.Add(modifier);
            _effectModifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
    }
    
    /// <summary>
    /// Unregisters a custom injury effect modifier.
    /// </summary>
    public void UnregisterEffectModifier(IInjuryEffectModifier modifier)
    {
        _effectModifiers.Remove(modifier);
    }
    
    /// <summary>
    /// Updates stat modifiers based on current injuries.
    /// </summary>
    private void UpdateStatModifiers()
    {
        // Start with fresh modifiers
        CharacterStatModifiers modifiers = new CharacterStatModifiers();
        
        // Get all injuries from circulation system
        Dictionary<string, LimbInjuries> allInjuries = _circulationSystem.GetAllInjuries();
        
        // Process each limb's injuries
        foreach (var kvp in allInjuries)
        {
            string limbName = kvp.Key;
            LimbInjuries injuries = kvp.Value;
            
            if (injuries.InjuryCount == 0) continue;
            
            // Get limb type
            LimbType limbType = LimbTypeMapper.GetLimbType(limbName);
            
            // Calculate cumulative severity for this limb
            float cumulativeSeverity = CalculateCumulativeSeverity(injuries);
            
            // Apply default preset modifiers
            CharacterStatModifiers presetModifiers = StatModifierPresets.GetModifiersForSeverity(
                InjurySeverityHelper.GetSeverityFromRatio(cumulativeSeverity),
                limbType
            );
            
            // Combine modifiers (multiply together)
            CombineModifiers(ref modifiers, presetModifiers);
            
            // Apply custom modifiers
            foreach (InjuryData injury in injuries.Injuries)
            {
                if (injury.Severity <= 0f) continue;
                
                foreach (IInjuryEffectModifier customModifier in _effectModifiers)
                {
                    if (customModifier.ShouldApply(injury, limbType))
                    {
                        customModifier.ApplyModifier(ref modifiers, injury, limbType);
                    }
                }
            }
        }
        
        // Apply blood volume penalty
        ApplyBloodVolumePenalty(ref modifiers);
        
        // Apply dismemberment penalties
        ApplyDismembermentPenalties(ref modifiers);
        
        // Clamp modifiers to reasonable range
        modifiers.Clamp(0.1f, 1.5f);
        
        // Check if modifiers changed
        if (!ModifiersEqual(_currentModifiers, modifiers))
        {
            _currentModifiers = modifiers;
            OnStatModifiersChanged?.Invoke(modifiers);
            
            // Apply modifiers to humanoid
            ApplyModifiersToHumanoid(modifiers);
        }
    }
    
    /// <summary>
    /// Calculates cumulative severity for a limb's injuries.
    /// </summary>
    private float CalculateCumulativeSeverity(LimbInjuries injuries)
    {
        float cumulative = 0f;
        
        for (int i = 0; i < injuries.InjuryCount; i++)
        {
            cumulative += injuries.Injuries[i].Severity;
        }
        
        // Cap at 1.0 (100%)
        return Mathf.Min(1.0f, cumulative);
    }
    
    /// <summary>
    /// Combines two sets of modifiers by multiplying them.
    /// </summary>
    private void CombineModifiers(ref CharacterStatModifiers target, CharacterStatModifiers source)
    {
        target.WalkSpeedMultiplier *= source.WalkSpeedMultiplier;
        target.AttackSpeedMultiplier *= source.AttackSpeedMultiplier;
        target.AttackStrengthMultiplier *= source.AttackStrengthMultiplier;
        target.StaminaRegenMultiplier *= source.StaminaRegenMultiplier;
        target.MaxStaminaMultiplier *= source.MaxStaminaMultiplier;
        target.DodgeSpeedMultiplier *= source.DodgeSpeedMultiplier;
        target.BlockStrengthMultiplier *= source.BlockStrengthMultiplier;
    }
    
    /// <summary>
    /// Applies blood volume penalty to modifiers.
    /// </summary>
    private void ApplyBloodVolumePenalty(ref CharacterStatModifiers modifiers)
    {
        float bloodRatio = _circulationSystem.BloodVolumeRatio;
        
        // Blood volume affects all stats progressively
        if (bloodRatio < 0.8f)
        {
            float penalty = Mathf.Lerp(1.0f, 0.5f, (0.8f - bloodRatio) / 0.8f);
            
            modifiers.WalkSpeedMultiplier *= penalty;
            modifiers.AttackSpeedMultiplier *= penalty;
            modifiers.AttackStrengthMultiplier *= penalty;
            modifiers.StaminaRegenMultiplier *= penalty;
        }
    }
    
    /// <summary>
    /// Applies penalties for dismembered limbs.
    /// </summary>
    private void ApplyDismembermentPenalties(ref CharacterStatModifiers modifiers)
    {
        // Check for missing limbs
        if (!_limbManager.IsLimbAttached("left hand") || !_limbManager.IsLimbAttached("right hand"))
        {
            modifiers.AttackSpeedMultiplier *= 0.7f;
            modifiers.BlockStrengthMultiplier *= 0.5f;
        }
        
        if (!_limbManager.IsLimbAttached("left forearm") || !_limbManager.IsLimbAttached("right forearm"))
        {
            modifiers.AttackStrengthMultiplier *= 0.6f;
            modifiers.BlockStrengthMultiplier *= 0.3f;
        }
        
        if (!_limbManager.IsLimbAttached("left upper arm") || !_limbManager.IsLimbAttached("right upper arm"))
        {
            modifiers.AttackStrengthMultiplier *= 0.5f;
            modifiers.BlockStrengthMultiplier *= 0.2f;
        }
        
        if (!_limbManager.IsLimbAttached("left foot") || !_limbManager.IsLimbAttached("right foot"))
        {
            modifiers.WalkSpeedMultiplier *= 0.8f;
            modifiers.DodgeSpeedMultiplier *= 0.7f;
        }
        
        if (!_limbManager.IsLimbAttached("left shin") || !_limbManager.IsLimbAttached("right shin"))
        {
            modifiers.WalkSpeedMultiplier *= 0.6f;
            modifiers.DodgeSpeedMultiplier *= 0.5f;
        }
        
        if (!_limbManager.IsLimbAttached("left thigh") || !_limbManager.IsLimbAttached("right thigh"))
        {
            modifiers.WalkSpeedMultiplier *= 0.4f;
            modifiers.DodgeSpeedMultiplier *= 0.3f;
        }
    }
    
    /// <summary>
    /// Applies modifiers to the humanoid character.
    /// </summary>
    private void ApplyModifiersToHumanoid(CharacterStatModifiers modifiers)
    {
        if (_humanoid == null) return;
        
        // Apply to character controller or similar
        // This would need to be implemented based on how the character handles stat modifications
        // For now, we'll just print the modifiers
        GD.Print($"Stat Modifiers - Walk: {modifiers.WalkSpeedMultiplier:F2}, " +
                 $"Attack Speed: {modifiers.AttackSpeedMultiplier:F2}, " +
                 $"Attack Strength: {modifiers.AttackStrengthMultiplier:F2}");
    }
    
    /// <summary>
    /// Checks if two modifier sets are equal.
    /// </summary>
    private bool ModifiersEqual(CharacterStatModifiers a, CharacterStatModifiers b)
    {
        const float epsilon = 0.001f;
        
        return Mathf.Abs(a.WalkSpeedMultiplier - b.WalkSpeedMultiplier) < epsilon &&
               Mathf.Abs(a.AttackSpeedMultiplier - b.AttackSpeedMultiplier) < epsilon &&
               Mathf.Abs(a.AttackStrengthMultiplier - b.AttackStrengthMultiplier) < epsilon &&
               Mathf.Abs(a.StaminaRegenMultiplier - b.StaminaRegenMultiplier) < epsilon &&
               Mathf.Abs(a.MaxStaminaMultiplier - b.MaxStaminaMultiplier) < epsilon &&
               Mathf.Abs(a.DodgeSpeedMultiplier - b.DodgeSpeedMultiplier) < epsilon &&
               Mathf.Abs(a.BlockStrengthMultiplier - b.BlockStrengthMultiplier) < epsilon;
    }
    
    /// <summary>
    /// Called when blood volume changes.
    /// </summary>
    private void OnBloodVolumeChanged(float current, float max)
    {
        // Trigger immediate update of stat modifiers
        UpdateStatModifiers();
    }
    
    /// <summary>
    /// Resets all stat modifiers to default values.
    /// </summary>
    public void ResetModifiers()
    {
        _currentModifiers.Reset();
        OnStatModifiersChanged?.Invoke(_currentModifiers);
        ApplyModifiersToHumanoid(_currentModifiers);
    }
}
