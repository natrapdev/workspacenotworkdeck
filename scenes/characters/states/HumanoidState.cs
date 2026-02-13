using Godot;
//using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidState : Node
{
	// public CharacterBody3D Character { get; set; }
	// public Node3D CharacterModel { get; set; }
	// public AnimationTree CharacterAnimationTree { get; set; }
	// public HumanoidResource CharacterResource { get; set; }
	// public StateData CharacterStateData { get; set; }
	// public Humanoid CharacterHumanoid { get; set; }
	// public Skeleton3D CharacterSkeleton { get; set; }
	// public BoneAttachment3D HeadBoneAttachment { get; set; }
	// public LookAtModifier3D HeadLookAt { get; set; }
	// public LookAtModifier3D BodyLookAt { get; set; }

	// public Animator CharacterAnimator { get; set; }
	// public Dictionary<string, HumanoidState> StateList { get; set; }

	// public float StaminaCost = 0f;
	// public float FatigueCost = 0f;

	// private float _duration;

	// private static readonly Dictionary<string, int> _statePriorities = new()
	// {
	// 	{"idle", 1},
	// 	{"walk", 2},
	// 	{"interact", 3},
	// 	{"unsheathe1", 4},
	// 	{"unsheathe2", 4},
	// 	{"airborne", 10},
	// 	{"jump", 10},
	// };

	// readonly Stopwatch stopwatch = new();

	// public virtual void Update(InputPackage input, float delta)
	// {

	// }

	// public virtual string CheckRelevance(InputPackage input)
	// {
	// 	return "OK";
	// }

	// public virtual string FindFirstValidState(InputPackage input)
	// {
	// 	var sortedInputs = SortInputActions(input.actions);

	// 	foreach (string action in sortedInputs)
	// 	{
	// 		if (CharacterResource.HasEnoughStamina(StateList[action]))
	// 		{
	// 			if (StateList[action].Equals(this))
	// 			{
	// 				return "OK";
	// 			}
	// 			else
	// 			{
	// 				return action;
	// 			}
	// 		}
	// 	}

	// 	return "Could not find an idle state";
	// }

	// public static List<string> SortInputActions(List<string> actions) => [.. actions.OrderByDescending(a => _statePriorities[a])];

	// public virtual void OnEnterState()
	// {

	// }

	// public virtual void OnExitState()
	// {

	// }

	// public void SetTimer()
	// {
	// 	if (stopwatch.IsRunning) stopwatch.Reset();

	// 	stopwatch.Start();
	// }

	// public float GetElapsedTimeMilliseconds() => stopwatch.ElapsedMilliseconds;
	// public bool ExceedsTimeLength(float time) => stopwatch.ElapsedMilliseconds > time;
}