using System;
using Godot;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Resources : Node
{
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaGain { get; set; } = 1.2f;
    [Export] public float FatigueGain { get; set; } = .1f;

    private float _currentStamina = 0;
    private float _currentFatigue = 0;

    public override void _Ready()
    {
        _currentStamina = MaxStamina;
    }

    public bool HasEnoughStamina(CharacterState state)
    {
        return state.staminaCost <= _currentStamina && _currentStamina >= 0;
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
}