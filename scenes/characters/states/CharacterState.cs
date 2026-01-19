using Godot;
//using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class CharacterState : Node
{
	public Dictionary<string, CharacterState> stateList;
	public CharacterBody3D character;
	public Node3D characterModel;
	public AnimationTree animationTree;
	public Resources characterResource = new();
	public StateData stateData;
	public Node3D camPivot;
	public float staminaCost = 0f;
	public float fatigueCost = 0f;

	public string defaultLocomotionPath = "parameters/DefaultLocomotion/blend_position";

	public static readonly Dictionary<string, int> statePriorities = new()
	{
		{"idle", 1},
		{"walk", 2},
		{"slashing", 3},
		{"stabbing", 3},
		{"airborne", 10},
		{"jump", 10},
	};

	double initialSystemTime = Time.GetUnixTimeFromSystem();
	public override void _Ready() // pleaselp elpeplease PLEASE change this soon
	{
		/*
		 * Purely out of by own laziness I am hardcoding all of these values in.
		 * This is not an optimal approach to this. Please change this soon.
		 * Thank you in advance. or if you don't do anything you had it coming bozo
		 * Love, natty p of 2025-01-19
		 */
		character = GetNode<CharacterBody3D>("../../..");
		characterModel = character.GetNode<Node3D>("HumanMan");
		camPivot = character.GetNode<Node3D>("CameraPivot");
		animationTree = GetNode<AnimationTree>("../../../AnimationTree");
	}

	public virtual void Update(InputPackage input, float delta)
	{

	}

	public virtual string CheckRelevance(InputPackage input)
	{
		return "OK";
	}

	public virtual string FindFirstValidState(InputPackage input)
	{
		var sortedInputs = SortInputActions(input.actions);

		foreach (string action in sortedInputs)
		{
			if (characterResource.HasEnoughStamina(stateList[action]))
			{
				if (stateList[action].Equals(this))
				{
					return "OK";
				}
				else
				{
					return action;
				}
			}
		}

		return "Could not find an idle state";
	}

	public List<string> SortInputActions(List<string> actions)
	{
		return [.. actions.OrderByDescending(action => statePriorities[action])];
	}

	public virtual void OnEnterState()
	{

	}

	public virtual void OnExitState()
	{

	}

	public float GetProgress()
	{
		double currentTime = Time.GetUnixTimeFromSystem();
		return (float)(currentTime - initialSystemTime);
	}

	public void SetInitialSystemTime()
	{
		initialSystemTime = Time.GetUnixTimeFromSystem();
	}

	public bool ExceedsTimeLength(float time)
	{
		return GetProgress() >= time;
	}
}