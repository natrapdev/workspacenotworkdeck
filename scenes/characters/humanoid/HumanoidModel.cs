using Godot;
using MyFirst3DGame.Items;
using MyFirst3DGame.scenes.characters.humanoid.injury;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using System;
using System.Threading.Tasks;

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
	[Export] public bool Debug { get; set; } = false;
	[Export] public InjurySystemManager InjurySystem { get; set; }

	public Weapon CurrentWeapon { get; set; }

	[Export] public HumanoidStates StateContainer { get; set; }
	public State CurrentState { get; set; }

	[Export] public WeaponInventory WeaponInventory { get; set; }
	public Inventory Inventory;

	public Node3D LookAtReference { get; set; }

	public override void _Ready()
	{
		Character = GetParent() as CharacterBody3D;
		StateContainer.Character = Character;
		StateContainer.AcceptStates();
		CurrentState = StateContainer.States["idle"];
		HumanoidLegs.CurrentState = CurrentState;
		HumanoidLegs.AcceptStates();

		if (Character.Name.ToString().Contains("Player"))
		{
			LookAtReference = GetParent().GetNode<Node3D>("CameraPivot");
		}
		else
		{
			LookAtReference = Resource.HeadBoneAttachment;
		}
		
		// Initialize injury system
		if (InjurySystem != null)
		{
			InjurySystem.InitializeInjurySystem(this);
			
			// Subscribe to injury system events
			InjurySystem.OnDamageProcessed += OnDamageProcessed;
			InjurySystem.OnLimbDismembered += OnLimbDismembered;
			InjurySystem.OnCharacterDeath += OnCharacterDeath;
			InjurySystem.OnStatModifiersChanged += OnStatModifiersChanged;
		}
	}

	public virtual async Task Update(InputPackage input, float delta)
	{
		input = Combat.Contextualize(input);

		var relevance = CurrentState.ChangeState(input);

		if (!relevance.Equals(CurrentState))
		{
			SwitchTo(relevance);
		}

		CurrentState.UpdateResource(delta);
		await CurrentState.Update(input, delta);
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
	
	/// <summary>
	/// Called when damage is processed by the injury system.
	/// </summary>
	private void OnDamageProcessed(DamageProcessingResult result)
	{
		if (Debug)
		{
			GD.Print($"Damage processed on {result.DamageResult.LimbName}: " +
				$"Severity {result.DamageResult.Severity:F2}, " +
				$"Dismember: {result.DamageResult.ShouldDismember}");
		}
	}
	
	/// <summary>
	/// Called when a limb is dismembered.
	/// </summary>
	private void OnLimbDismembered(DismembermentResult result)
	{
		if (Debug)
		{
			GD.Print($"Limb dismembered: {result.LimbName}");
		}
		
		// Apply any visual effects for dismemberment
		// (e.g., blood spray, sound effects)
	}
	
	/// <summary>
	/// Called when the character dies.
	/// </summary>
	private void OnCharacterDeath(DeathSystem.DeathCondition? cause)
	{
		if (Debug)
		{
			GD.Print($"Character died: {DeathSystem.GetDeathCauseDescription(cause ?? DeathSystem.DeathCondition.TotalBloodBelow40Percent)}");
		}
		
		// Stop any ongoing actions
		// Disable input
	}
	
	/// <summary>
	/// Called when stat modifiers change due to injuries.
	/// </summary>
	private void OnStatModifiersChanged(CharacterStatModifiers modifiers)
	{
		if (Debug)
		{
			GD.Print($"Stat modifiers changed - Walk: {modifiers.WalkSpeedMultiplier:F2}, " +
				$"Attack Speed: {modifiers.AttackSpeedMultiplier:F2}, " +
				$"Attack Strength: {modifiers.AttackStrengthMultiplier:F2}");
		}
		
		// Apply modifiers to character controller
		// This would need to be implemented based on how the character handles stat modifications
	}
}