using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class StateModel : Node
{
	Dictionary<string, CharacterState> states = [];
	string currentStateName;
	private CharacterBody3D _character;
	private AnimationTree _animationTree;
	private Resources _characterResource;
	private StateData _stateData;

	public override void _Ready()
	{
		states.Add("idle", GetNodeOrNull<Idle>("Idle"));
		states.Add("walk", GetNodeOrNull<Walk>("Walk"));
		states.Add("jump", GetNodeOrNull<Jump>("Jump"));
		states.Add("airborne", GetNodeOrNull<Airborne>("Airborne"));

		currentStateName = "idle";

		foreach (var state in states.Values)
		{
		// 	state.character = _character;
		// 	state.characterResource = _characterResource;
		// 	state.stateData = _stateData;
		 	state.stateList = states;
		}

		// foreach (var child in GetChildren())
		// {
		// 	if (child is CharacterState)
		// 	{
				
		// 	}
		// }
	}

	public virtual void Update(InputPackage input, float delta)
	{
		string relevance = states[currentStateName].CheckRelevance(input);
		
		if (!relevance.Equals("OK"))
		{
			SwitchTo(relevance);
		}
		states[currentStateName].Update(input, delta);
	}

	public virtual void SwitchTo(string state)
	{
		states[currentStateName].OnExitState();
		currentStateName = state;
		states[currentStateName].OnEnterState();
	}
}
