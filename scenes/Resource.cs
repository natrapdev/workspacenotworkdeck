using Godot;
using System;
using System.Collections.Generic;
using System.Data;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Resource : Node
{
    [Export] public bool GodMode { get; set; } = false;

    [Export] public float BodyMass { get; set; } = 68f;
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaRegen { get; set; } = 3f;
	[Export] public float FatigueGain { get; set; } = .1f;

    
    private float stamina;
    private List<string> _statuses = [];
    private float _bloodVolume;
    private float _heartRate; // beats per minute
    private float _strokeVolume;
    private float _cardiacOutput;

    // How BodyMass should be distributed across the body
    private readonly Dictionary<string, float> _bodyPartWeightPercentage = new()
    {
        {"head", 0.0826f},
        {"thorax", 0.2010f},
        {"abdomen", 0.1310f},
        {"pelvis", 0.1370f},
        {"upper arm", 0.0325f}, // We usually have two of these so multiply by 2 for total
        {"lower arm", 0.0187f - 0.0006f}, // All body parts add up to 1.0006 so this is to make it consitent
        {"hand", 0.0065f},
        {"thigh", 0.1050f},
        {"shin", 0.0475f},
        {"foot", 0.0143f}
    };

    public override void _Ready()
    {
        stamina = MaxStamina;
    }

    public void Update(float delta)
    {
        stamina += StaminaRegen * delta;
    }

    public void UpdateStamina(float change)
    {
        stamina += change;
    }
    public bool HasEnoughStamina(CharacterState action)
    {
        return action.staminaCost < stamina || stamina > 0; 
    }
    public float CalculateBloodVolume(string sex)
    {
        return BodyMass * (sex.ToLower().Contains('f') ? 65f : 75f);
    }
}
