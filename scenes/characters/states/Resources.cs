using System;
using System.Collections.Generic;
using System.Text;
using Godot;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Resources : Node
{
    [Export] public bool GodMode { get; set; } = false;
    [Export] public float BodyMass { get; set; } = 68f; // kg
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaGain { get; set; } = 3f;
	[Export] public float FatigueGain { get; set; } = .1f;

    private List<string> _statuses = [];
    private float _totalBloodVolume;
    private float _heartRate; // beats per minute
    private float _strokeVolume;
    private float _cardiacOutput;
    private float _currentStamina = 1f;
    private float _currentFatigue = 0;

    // How body mass should be distributed across the body
    private readonly Dictionary<string, float> _bodyPartMassCoefficients = new()
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

    private Dictionary<string, float> _bloodVolumeInBodyPart;


    public override void _Ready()
    {
        _currentStamina = MaxStamina;
    }

    public bool HasEnoughStamina(CharacterState state)
    {
        return state.staminaCost <= _currentStamina && _currentStamina > 0;
    }

    public void Update()
    {
        ChangeStamina(StaminaGain);
    }

    public void ChangeStamina(float changeValue)
    {
        _currentFatigue = Mathf.Clamp(_currentFatigue + changeValue, -MaxStamina, MaxStamina);
    }
    public void ChangeFatigue(float changeValue)
    {
        _currentFatigue += changeValue;
    }
    public float CalculateTotalBloodVolume()
    {
        return BodyMass * 75f; // blood in mL/kg
    }

    public float CalculateBodyPartBloodVolume(string bodyPart)
    {
        return CalculateTotalBloodVolume() * _bodyPartMassCoefficients[bodyPart];
    }

    public float CalculateBodyPartMass(string bodyPart)
    {
        return BodyMass * _bodyPartMassCoefficients[bodyPart];
    }


}