using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class StateModel : Node
{
	public Dictionary<string, CharacterState> States = [];
	public string CurrentStateName { get; set; }
	public CharacterBody3D Character { get; private set; }
	public Node3D CharacterModel { get; private set; }
	public AnimationTree CharacterAnimationTree { get; private set; }
	public Resources CharacterResource { get; private set; }
	public StateData CharacterStateData { get; private set; }
	[Export] public Humanoid CharacterHumanoid { get; set; }
	public Skeleton3D CharacterSkeleton { get; private set; }
	[Export] public Animator CharacterAnimator { get; set; }

	public override void _Ready()
	{
		CharacterHumanoid.Ready += OnHumanoidReady;
	}

	private void OnHumanoidReady()
	{
		Character = CharacterHumanoid.Character;
		CharacterModel = CharacterHumanoid.CharacterModel;
		CharacterResource = CharacterHumanoid.GetNode<Resources>("Resource");
		CharacterResource.Character = Character;
		CharacterResource.CharacterModel = CharacterModel;
		CharacterResource.OnReady();
		CharacterStateData = GetNode<StateData>("StateData");
		CharacterSkeleton = CharacterHumanoid.Skeleton;
		CharacterAnimationTree = CharacterHumanoid.AnimationTree;

		// States list might not be necessary
		States.Add("idle", GetNodeOrNull<Idle>("Idle"));
		States.Add("walk", GetNodeOrNull<Walk>("Walk"));
		States.Add("jump", GetNodeOrNull<Jump>("Jump"));
		States.Add("airborne", GetNodeOrNull<Airborne>("Airborne"));
		States.Add("interact", GetNodeOrNull<InteractWithItem>("InteractWithItem"));
		States.Add("unsheathe1", GetNodeOrNull<UnsheathePrimary>("UnsheathePrimary"));

		CurrentStateName = "idle";

		foreach (var child in GetChildren())
		{
			if (child is CharacterState state)
			{
				state.Character = Character;
				state.CharacterModel = CharacterModel;
				state.CharacterAnimationTree = CharacterAnimationTree;
				state.CharacterResource = CharacterResource;
				state.CharacterStateData = CharacterStateData;
				state.CharacterHumanoid = CharacterHumanoid;
				state.CharacterSkeleton = CharacterSkeleton;
				state.HeadBoneAttachment = CharacterSkeleton.GetNode<BoneAttachment3D>("HeadBoneAttachment");
				state.HeadLookAt = CharacterSkeleton.GetNode<LookAtModifier3D>("HeadLookAt");
				state.StateList = States;
				state.CharacterAnimator = CharacterAnimator;
			}
		}
	}

	public virtual void Update(InputPackage input, float delta)
	{
		CharacterResource.Update();
		string relevance = States[CurrentStateName].CheckRelevance(input);

		if (!relevance.Equals("OK"))
		{
			SwitchTo(relevance);
		}
		States[CurrentStateName].Update(input, delta);
	}

	public virtual void SwitchTo(string state)
	{
		States[CurrentStateName].OnExitState();
		CurrentStateName = state;
		States[CurrentStateName].OnEnterState();
	}
}
