using Godot;
using MyFirst3DGame.Items;
using System;
using System.Threading.Tasks;
using MyFirst3DGame.scenes.characters.humanoid;

namespace MyFirst3DGame.scenes.characters.states;

public sealed partial class HumanoidModel : Node3D
{
	[Export] public int Team { get; set; } = 0;
	public CharacterBody3D Character { get; set; }
	[Export] public Skeleton3D Skeleton { get; set; }
	[Export] public Animator Animator { get; set; }
	[Export] public HumanoidCombat Combat { get; set; }
	[Export] public HumanoidResource Resource { get; set; }
	[Export] public Marker3D HeadLookAtTarget { get; set; }
	[Export] public HumanoidLegStates HumanoidLegs { get; set; }
	[Export] public DamageModel DamageModel { get; set; }
	[Export] public bool Debug { get; set; } = false;

	public Weapon CurrentWeapon { get; set; }

	[Export] public HumanoidStates StateContainer { get; set; }
	public State CurrentState { get; private set; }

	[Export] public WeaponInventory WeaponInventory { get; set; }
	public Inventory Inventory;

	public Node3D LookAtReference { get; private set; }

	public override void _Ready()
	{
		Character = GetParent() as CharacterBody3D;
		StateContainer.Character = Character;
		StateContainer.AcceptStates();
		CurrentState = StateContainer.States["idle"];
		HumanoidLegs.CurrentState = CurrentState;
		HumanoidLegs.AcceptStates();

		if (Character != null && Character.Name.ToString().Contains("Player"))
		{
			LookAtReference = GetParent().GetNode<Node3D>("CameraPivot");
		}
		else
		{
			LookAtReference = Resource.HeadBoneAttachment;
		}
		
		// DamageModel.Initialize();
	}

	public void Update(InputPackage input, float delta)
	{
		input = Combat.Contextualize(input);
		
		State relevance = CurrentState.ChangeState(input);
		
		if (!relevance.Equals(CurrentState)) SwitchTo(relevance);
		
		CurrentState.UpdateResource(delta);
		CurrentState.Update(input, delta);
		DamageModel.Update(delta);
	}

	public void SwitchTo(State state)
	{
		if (Debug) GD.Print(CurrentState.StateName + " -> " + state.StateName);

		CurrentState.Exit();
		CurrentState = state;
		CurrentState.Enter();
	}

	public void SwitchTo(string stateName)
	{
		State state = StateContainer.GetStateByName(stateName);

		if (state is not null) SwitchTo(state);
	}

	public void MoveHeadLookAtTarget(Vector3 pos) =>
	GetNode<Marker3D>("HeadLookAtTarget").GlobalPosition = pos;
}