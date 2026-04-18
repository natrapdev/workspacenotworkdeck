# Injury and Death System - Setup Guide

This guide explains how to set up and use the injury and death system in your Godot 4.6.1 project.

## Overview

The injury and death system provides:
- **Asynchronous damage processing** - Damage calculations run in background tasks
- **Limb dismemberment** - Detached limbs become physics objects
- **Blood circulation** - Pseudo blood flow between connected limbs
- **Injury effects** - Limb injuries affect character stats
- **Death conditions** - Automatic ragdoll trigger on death

## Scene Setup

### 1. Add InjurySystemManager to HumanoidModel

Open your humanoid scene (e.g., `humanoid.tscn`) and add the InjurySystemManager:

1. Select the `HumanoidModel` node
2. Add a child node: `InjurySystemManager` (type: `InjurySystemManager`)
3. In the Inspector, drag the new `InjurySystemManager` to the `InjurySystem` export property

### 2. Verify Required Nodes

Ensure your humanoid scene has these required nodes:

```
HumanoidModel (Node3D)
├── Skeleton (HumanoidSkeleton)
├── DamageModel (DamageModel)
├── Resource (HumanoidResource)
├── Combat (HumanoidCombat)
├── InjurySystem (InjurySystemManager) ← Add this
└── ... other nodes
```

### 3. DamageModel Setup

The `DamageModel` must have `BoneAttachment3D` children for each limb:

```
DamageModel (Node3D)
├── LeftHand (BoneAttachment3D)
│   └── CollisionShape3D
├── RightHand (BoneAttachment3D)
│   └── CollisionShape3D
├── LeftForearm (BoneAttachment3D)
│   └── CollisionShape3D
├── RightForearm (BoneAttachment3D)
│   └── CollisionShape3D
├── LeftUpperArm (BoneAttachment3D)
│   └── CollisionShape3D
├── RightUpperArm (BoneAttachment3D)
│   └── CollisionShape3D
├── Thorax (BoneAttachment3D)
│   └── CollisionShape3D
├── Head (BoneAttachment3D)
│   └── CollisionShape3D
├── Neck (BoneAttachment3D)
│   └── CollisionShape3D
├── Abdomen (BoneAttachment3D)
│   └── CollisionShape3D
├── Pelvis (BoneAttachment3D)
│   └── CollisionShape3D
├── LeftThigh (BoneAttachment3D)
│   └── CollisionShape3D
├── RightThigh (BoneAttachment3D)
│   └── CollisionShape3D
├── LeftShin (BoneAttachment3D)
│   └── CollisionShape3D
├── RightShin (BoneAttachment3D)
│   └── CollisionShape3D
├── LeftFoot (BoneAttachment3D)
│   └── CollisionShape3D
└── RightFoot (BoneAttachment3D)
    └── CollisionShape3D
```

**Important Notes:**
- Each `BoneAttachment3D` must be attached to the correct bone in the skeleton
- Each must have a `CollisionShape3D` child with the appropriate shape
- The `Area3D` collision layer should be set to layer 2 (damage layer)

### 4. HumanoidResource Setup

Ensure `HumanoidResource` has the body part mass coefficients:

```csharp
// In HumanoidResource.cs, these should already be defined:
public readonly Dictionary<string, float> BodyPartMassCoefficients = new()
{
    {"head", 0.0526f},
    {"neck", 0.03f},
    {"thorax", 0.2010f},
    {"abdomen", 0.1310f},
    {"pelvis", 0.1370f},
    {"upper arm", 0.0325f},
    {"forearm", 0.0181f},
    {"hand", 0.0065f},
    {"thigh", 0.1050f},
    {"shin", 0.0475f},
    {"foot", 0.0143f}
};
```

## Usage

### Basic Usage

The injury system is automatically initialized when the humanoid is ready. No additional setup is required in code.

### Subscribing to Events

You can subscribe to injury system events in your character controller:

```csharp
public partial class MyCharacterController : Node
{
    private InjurySystemManager _injurySystem;
    
    public override void _Ready()
    {
        HumanoidModel humanoid = GetParent<HumanoidModel>();
        _injurySystem = humanoid.InjurySystem;
        
        // Subscribe to events
        _injurySystem.OnDamageProcessed += OnDamageProcessed;
        _injurySystem.OnLimbDismembered += OnLimbDismembered;
        _injurySystem.OnCharacterDeath += OnCharacterDeath;
        _injurySystem.OnStatModifiersChanged += OnStatModifiersChanged;
    }
    
    private void OnDamageProcessed(DamageProcessingResult result)
    {
        GD.Print($"Damage processed: {result.DamageResult.LimbName}");
        // Play hit effects, sounds, etc.
    }
    
    private void OnLimbDismembered(DismembermentResult result)
    {
        GD.Print($"Limb dismembered: {result.LimbName}");
        // Play dismemberment effects, blood spray, etc.
    }
    
    private void OnCharacterDeath(DeathSystem.DeathCondition? cause)
    {
        GD.Print($"Character died: {cause}");
        // Handle death (disable input, show death screen, etc.)
    }
    
    private void OnStatModifiersChanged(CharacterStatModifiers modifiers)
    {
        // Apply modifiers to your character controller
        // Example:
        characterController.WalkSpeed = baseWalkSpeed * modifiers.WalkSpeedMultiplier;
        characterController.AttackSpeed = baseAttackSpeed * modifiers.AttackSpeedMultiplier;
    }
}
```

### Custom Injury Effect Modifiers

Create custom injury effect modifiers to add new gameplay mechanics:

```csharp
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using MyFirst3DGame.scenes.characters.humanoid.injury.interfaces;

public partial class MyCustomModifier : Node, IInjuryEffectModifier
{
    public float Priority => 10.0f;
    
    public bool ShouldApply(InjuryData injury, LimbType limbType)
    {
        // Return true if this modifier should apply
        return injury.Severity > 0.5f;
    }
    
    public void ApplyModifier(ref CharacterStatModifiers modifiers, InjuryData injury, LimbType limbType)
    {
        // Modify the stat modifiers
        modifiers.WalkSpeedMultiplier *= 0.8f;
    }
}
```

Register the custom modifier:

```csharp
public override void _Ready()
{
    HumanoidModel humanoid = GetParent<HumanoidModel>();
    InjurySystemManager injurySystem = humanoid.InjurySystem;
    
    // Create and register custom modifier
    MyCustomModifier modifier = new MyCustomModifier();
    injurySystem.RegisterInjuryEffectModifier(modifier);
}
```

### Forcing Dismemberment

You can force dismemberment of a limb (for scripted events):

```csharp
_injurySystem.ForceDismemberLimb("left hand");
```

### Forcing Death

You can force death with a specific cause:

```csharp
_injurySystem.ForceDeath(DeathSystem.DeathCondition.TotalBloodBelow40Percent);
```

### Checking Health State

Get the current health state:

```csharp
HealthState health = _injurySystem.GetHealthState();
GD.Print($"Blood: {health.CurrentBloodVolume}/{health.TotalBloodVolume}");
GD.Print($"Thorax: {health.ThoraxBloodRatio:P0}");
GD.Print($"Head: {health.HeadBloodRatio:P0}");
```

## Death Conditions

The character dies when any of these conditions are met:

1. **Total blood volume below 40%** - Character has lost too much blood
2. **Thorax blood volume below 30%** - Critical chest injury
3. **Head blood volume at 0%** - Decapitation or fatal head injury

When death occurs:
- Ragdoll is triggered on the skeleton
- Character switches to "dead" state
- All damage processing stops

## Blood Circulation

The system uses a pseudo blood circulation model:

```
Thorax (heart)
├── Head
├── Neck
├── Abdomen
│   └── Pelvis
│       ├── Left Thigh → Left Shin → Left Foot
│       └── Right Thigh → Right Shin → Right Foot
├── Left Upper Arm → Left Forearm → Left Hand
└── Right Upper Arm → Right Forearm → Right Hand
```

When a limb is injured:
- The injured limb loses blood at full rate
- Connected limbs lose blood at reduced rates
- Reduction increases with distance from injury

## Dismemberment

Limb dismemberment occurs when:
- Cut depth exceeds limb thickness threshold
- Threshold varies by limb type (hands/feet: 75%, arms/legs: 80-85%)

When dismembered:
- Limb becomes a separate physics object
- Visual limb is hidden from skeleton
- Bone is scaled to zero
- Injuries for that limb are cleared

## Performance Considerations

1. **Async Processing**: Damage is processed asynchronously to avoid frame drops
2. **Object Pooling**: Consider pooling detached limb objects for performance
3. **Update Frequency**: Circulation updates every frame, but could be throttled
4. **Cleanup**: Detached limbs are automatically cleaned up after 30 seconds

## Troubleshooting

### Injury system not initializing
- Verify `InjurySystem` export is set on `HumanoidModel`
- Check that `DamageModel`, `Skeleton`, and `Resource` nodes exist
- Ensure `DamageModel.OnReady()` has been called

### Dismemberment not working
- Verify limb thickness is set correctly in `DamageModule`
- Check that cut depth calculations are accurate
- Ensure `BoneAttachment3D` nodes are attached to correct bones

### Stat modifiers not applying
- Subscribe to `OnStatModifiersChanged` event
- Apply modifiers to your character controller
- Check that `InjuryEffectSystem` is processing

### Death not triggering
- Verify blood volume calculations are correct
- Check that `HumanoidSkeleton.Ragdoll()` is implemented
- Ensure death state exists in state container

## Examples

See `characters/humanoid/injury/examples/CustomInjuryEffectModifier.cs` for example custom modifiers including:
- Head/Neck/Thorax injury effects
- Bleeding effects
- Pain effects
- Adrenaline effects

## Architecture

The system consists of these components:

- **InjurySystemManager** - Central coordinator
- **DamageProcessor** - Async damage handling
- **CirculationSystem** - Blood circulation
- **LimbManager** - Limb state and dismemberment
- **DeathSystem** - Death condition checking
- **InjuryEffectSystem** - Stat modifiers

All data is stored in structs for cache efficiency, following data-oriented design principles.
