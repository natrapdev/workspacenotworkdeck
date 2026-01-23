using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class StateModel : Node
{
	Dictionary<string, CharacterState> states = [];
	string currentStateName;
	public CharacterBody3D Character { get; set; }
	public Node3D CharacterModel { get; set; }
	public AnimationTree CharacterAnimationTree { get; set; }
	public Resources CharacterResource { get; set; }
	public StateData CharacterStateData { get; set; }
	public Humanoid CharacterHumanoid { get; set; }

	public override void _Ready()
	{
		CharacterHumanoid = GetNode<Humanoid>("../");
		Character = GetNode<CharacterBody3D>("../..");
		CharacterModel = Character.GetNode<Node3D>("HumanMan");
		CharacterResource = GetNode<Resources>("../Resource");
		CharacterResource.character = Character;
		CharacterResource.characterModel = CharacterModel;
		CharacterResource.OnReady();
		CharacterStateData = GetNode<StateData>("StateData");

		// states list might not be necessary
		states.Add("idle", GetNodeOrNull<Idle>("Idle"));
		states.Add("walk", GetNodeOrNull<Walk>("Walk"));
		states.Add("jump", GetNodeOrNull<Jump>("Jump"));
		states.Add("airborne", GetNodeOrNull<Airborne>("Airborne"));
		states.Add("interact", GetNodeOrNull<InteractWithItem>("InteractWithItem"));

		currentStateName = "idle";

		foreach (Node child in GetChildren())
		{
			if (child is CharacterState)
			{
				CharacterState state = child as CharacterState;

				state.character = Character;
				state.characterModel = CharacterModel;
				state.characterResource = CharacterResource;
				state.stateData = CharacterStateData;
				state.CharacterHumanoid = CharacterHumanoid;
				state.stateList = states;

				// the walk state depends on cam pivot i dont want that anymore
				state.camPivot = Character.GetNode<Node3D>("CameraPivot"); // GET RID OF THIS DEPENDENCY ASAP!!!
			}
		}
	}

	public virtual void Update(InputPackage input, float delta)
	{
		CharacterResource.Update();
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
