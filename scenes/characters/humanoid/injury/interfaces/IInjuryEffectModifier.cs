using MyFirst3DGame.scenes.characters.humanoid.injury.data;

namespace MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;

/// <summary>
/// Interface for custom injury effect modifiers.
/// Allows extensibility for adding new stat modifiers based on injuries.
/// </summary>
public interface IInjuryEffectModifier
{
    /// <summary>
    /// Applies the modifier to the character stat modifiers.
    /// </summary>
    /// <param name="modifiers">Reference to the stat modifiers to modify</param>
    /// <param name="injury">The injury data to base the modifier on</param>
    /// <param name="limbType">The type of limb affected</param>
    void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType);
    
    /// <summary>
    /// Priority determines the order in which modifiers are applied.
    /// Higher priority modifiers are applied later.
    /// </summary>
    float Priority { get; }
    
    /// <summary>
    /// Determines if this modifier should be applied for the given injury.
    /// </summary>
    bool ShouldApply(InjuryData injury, LimbType limbType);
}
