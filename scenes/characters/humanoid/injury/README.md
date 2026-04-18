# Injury and Death System

A comprehensive, data-oriented injury and death system for Godot 4.6.1 .NET projects.

## Quick Start

1. Add `InjurySystemManager` as a child of your `HumanoidModel` node
2. Set the `InjurySystem` export property on `HumanoidModel`
3. Run your scene - the system auto-initializes

See [`INJURY_SYSTEM_SETUP.md`](INJURY_SYSTEM_SETUP.md) for detailed setup instructions.

## Features

- **Asynchronous Damage Processing** - Smooth gameplay with background damage calculations
- **Limb Dismemberment** - Detached limbs become physics objects
- **Blood Circulation** - Realistic blood flow between connected limbs
- **Injury Effects** - Limb injuries affect walkspeed, attack speed, and strength
- **Death Conditions** - Automatic ragdoll trigger on death
- **Extensible** - Easy to add custom injury modifiers

## Death Conditions

The character dies when:
- Total blood volume below 40%
- Thorax blood volume below 30%
- Head blood volume at 0%

## Documentation

- [`INJURY_SYSTEM_SETUP.md`](INJURY_SYSTEM_SETUP.md) - Complete setup guide
- [`IMPLEMENTATION_SUMMARY.md`](IMPLEMENTATION_SUMMARY.md) - Implementation overview
- [`plans/injury_death_system_plan.md`](../../plans/injury_death_system_plan.md) - Architecture design

## Examples

See [`examples/CustomInjuryEffectModifier.cs`](examples/CustomInjuryEffectModifier.cs) for example custom modifiers.

## Architecture

```
InjurySystemManager (Coordinator)
├── CirculationSystem (Blood flow)
├── DamageProcessor (Async damage)
├── LimbManager (Dismemberment)
├── DeathSystem (Death conditions)
└── InjuryEffectSystem (Stat modifiers)
```

## License

Part of the Retribuo project.
