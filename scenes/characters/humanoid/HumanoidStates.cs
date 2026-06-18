using Godot; 
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class HumanoidStates : Node
{
	public CharacterBody3D Character { get; set; }
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public HumanoidSkeleton Skeleton { get; set; }
	[Export] public Animator Animator { get; set; }
	[Export] public HumanoidCombat Combat { get; set; }
	[Export] public HumanoidResource Resource { get; set; }
	[Export] public StateData StateData { get; set; }
	[Export] public HumanoidLegStates HumanoidLegs { get; set; }

	public Dictionary<string, State> States { get; } = [];

	public override void _Ready() => Character = Humanoid.Character;

	public void AcceptStates()
	{
		foreach (Node child in GetChildren())
		{
			if (child is State state)
			{
				States.Add(state.StateName, state);
				SetProperties(state, state);
			}
		}
	}

	public static void CreateStatePipeline(State state)
	{
		state.NextState = null;

		if (state.GetChildCount() > 0)
		{
			State child = state.GetChild<State>(0);
			CreateStatePipeline(state.NextState = child);
		}
	}

	private void SetProperties(State baseState, State state)
	{
		int count = 0;

		state.Humanoid = Humanoid;
		state.Character = Character;
		state.Animator = Animator;
		state.Skeleton = Skeleton;
		state.Resource = Resource;
		state.Combat = Combat;
		state.StateData = StateData;
		state.Parent = this;
		state.Duration = StateData.GetDuration(state.BackendAnimation);
		state.NextState = state.GetChildOrNull<State>(0);
		state.HumanoidLegs = HumanoidLegs;
		state.CharacterModel = Humanoid.GetNode<Node3D>("CharacterModel");

		foreach (Node child in state.GetChildren())
		{
			if (child is State parentedState)
			{
				state.FollowUpStates.Add(parentedState);
			}
		}

		if (state is IChildState childState)
		{
			childState.BaseState = baseState;
		}

		if (state.NextState is not null)
		{
			GD.Print($"{state.StateName}'s next state: {state.NextState.StateName}");
			SetProperties(baseState, state.NextState, ++count);
		}
	}

	private void SetProperties(State baseState, State state, int childCount)
	{
		GD.Print($"State {state.StateName} is in layer {childCount}");

		state.Humanoid = Humanoid;
		state.Character = Character;
		state.Animator = Animator;
		state.Skeleton = Skeleton;
		state.Resource = Resource;
		state.Combat = Combat;
		state.StateData = StateData;
		state.Parent = this;
		state.Duration = StateData.GetDuration(state.BackendAnimation);
		state.NextState = state.GetChildOrNull<State>(0);
		state.HumanoidLegs = HumanoidLegs;
		state.CharacterModel = Humanoid.GetNode<Node3D>("CharacterModel");

		foreach (Node child in state.GetChildren())
		{
			if (child is State parentedState)
			{
				state.FollowUpStates.Add(parentedState);
			}
		}

		if (state is IChildState childState)
		{
			childState.BaseState = baseState;
		}

		if (state.NextState is not null)
		{
			SetProperties(baseState, state.NextState, ++childCount);
		}
	}

	public State GetStateByName(string name) => States[name];
}