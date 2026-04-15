using Godot;
using System;
using MyFirst3DGame.scenes.characters.states;

namespace MyFirst3DGame.scenes.characters.bot;

public partial class HumanoidBotModel : Node3D
{
	[Export] public int Team { get; set; } = 0;
	public CharacterBody3D Character { get; set; }
	[Export] public Skeleton3D Skeleton { get; set; }
	[Export] public Animator Animator { get; set; }
	[Export] public HumanoidResource Resource { get; set; }
	[Export] public Marker3D HeadLookAtTarget { get; set; }
	[Export] public HumanoidLegStates HumanoidLegs { get; set; }
	[Export] public HumanoidStates StateContainer { get; set; }
	[Export] public HumanoidCombat Combat { get; set; }
	[Export] public bool Debug { get; set; } = false;

	public State CurrentState { get; set; }

	public override void _Ready()
	{
		Character = GetParent() as CharacterBody3D;
		StateContainer.Character = Character;
		StateContainer.AcceptStates();
		CurrentState = StateContainer.States["idle"];
		// HumanoidLegs.CurrentState = CurrentState;
		// HumanoidLegs.AcceptStates();
	}

	public virtual void Update(InputPackage input, float delta)
	{
		input = Combat.Contextualize(input);

		var relevance = CurrentState.ChangeState(input);

		if (!relevance.Equals(CurrentState))
		{
			SwitchTo(relevance);
		}

		CurrentState.UpdateResource(delta);
		CurrentState.Update(input, delta);
	}

	public void SwitchTo(State state)
	{
		if (Debug) GD.Print(CurrentState.StateName + " -> " + state.StateName);

		CurrentState.Exit();
		CurrentState = state;
		CurrentState.Enter();
	}

}
