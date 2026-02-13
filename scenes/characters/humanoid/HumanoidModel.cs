using Godot;
using MyFirst3DGame.Items;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidModel : Node3D
{
	[Export] public int Team { get; set; } = 0;
	public CharacterBody3D Character { get; set; }
	[Export] public Skeleton3D Skeleton { get; set; }
	[Export] public Animator Animator { get; set; }
	[Export] public HumanoidCombat Combat { get; set; }
	[Export] public HumanoidResource Resource { get; set; }
	[Export] public Marker3D HeadLookAtTarget { get; set; }
	[Export] public HumanoidLegStates HumanoidLegs { get; set; }

	public Weapon CurrentWeapon { get; set; }

	[Export] public HumanoidStates StateContainer { get; set; }
	public State CurrentState { get; set; }

	[Export] public WeaponInventory WeaponInventory { get; set; }
	public Inventory Inventory;

	public override void _Ready()
	{
		Character = GetParent() as CharacterBody3D;
		StateContainer.Character = Character;
		StateContainer.AcceptStates();
		CurrentState = StateContainer.States["idle"];
		HumanoidLegs.CurrentState = CurrentState;
		HumanoidLegs.AcceptStates();
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
		GD.Print(CurrentState.StateName + " -> " + state.StateName);

		CurrentState.Exit();
		CurrentState = state;
		CurrentState.Enter();
	}

	public void MoveHeadLookAtTarget(Vector3 pos) => GetNode<Marker3D>("HeadLookAtTarget").GlobalPosition = pos;

}