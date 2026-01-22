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
	private Node3D _characterModel;
	private AnimationTree _animationTree;
	private Resources _characterResource;
	private StateData _stateData;

	public override void _Ready()
	{
		_character = GetNode<CharacterBody3D>("../..");
		_characterModel = _character.GetNode<Node3D>("HumanMan");
		_characterResource = GetNode<Resources>("../Resource");
		_characterResource.character = _character;
		_characterResource.characterModel = _characterModel;
		_stateData = GetNode<StateData>("StateData");

		// states list might not be necessary
		states.Add("idle", GetNodeOrNull<Idle>("Idle"));
		states.Add("walk", GetNodeOrNull<Walk>("Walk"));
		states.Add("jump", GetNodeOrNull<Jump>("Jump"));
		states.Add("airborne", GetNodeOrNull<Airborne>("Airborne"));
		states.Add("interact_with_item", GetNodeOrNull<InteractWithItem>("InteractWithItem"));

		currentStateName = "idle";

		foreach (Node child in GetChildren())
		{
			if (child is CharacterState)
			{
				CharacterState state = child as CharacterState;

				state.character = _character;
				state.characterModel = _characterModel;
				state.characterResource = _characterResource;
				state.stateData = _stateData;
				state.stateList = states;

				// the walk state depends on cam pivot i dont want that anymore
				state.camPivot = _character.GetNode<Node3D>("CameraPivot"); // GET RID OF THIS DEPENDENCY ASAP!!!
			}
		}
	}

	public virtual void Update(InputPackage input, float delta)
	{
		_characterResource.Update();
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
