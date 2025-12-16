using Godot;
using System;

public partial class GameInterface : CanvasLayer
{
	[Signal] public delegate void UpdateStaminaEventHandler(float stamina);
	
	[Export] public ProgressBar StaminaBar { get; set; }


	float stamina = 100;
	public override void _Ready()
    {
        UpdateStamina += StaminaChanged;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		StaminaBar.Value = stamina * 100;
    }

	public void StaminaChanged (float newStamina) => stamina = newStamina;
}
