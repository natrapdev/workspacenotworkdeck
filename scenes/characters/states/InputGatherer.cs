using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.states;

public partial class InputGatherer : Node
{
	[Export] public HumanoidModel Humanoid { get; set; }
	private float _startTime, _endTime, _avgTime, _shortTime, _longTime;

	private readonly Dictionary<string, int> _combatActionPriorities = new()
	{
		{"unsheathe1", 1},
		{"unsheathe2", 2},
		{"slash_prepare", 3},
		{"attack1", 4},
		{"attack2", 5},
		{"attack3", 5},
	};

	public override void _Ready()
	{
		base._Ready();
	}

	public InputPackage GatherInput()
	{
		List<string> actions = [];
		List<string> combatActions = [];

		actions.Add("idle");

		Vector2 inputDirection = Input.GetVector("move_right", "move_left", "move_back", "move_forward");

		if (inputDirection != Vector2.Zero)
		{
			actions.Add("walk");
		}

		if (Input.IsActionJustPressed("jump"))
		{
			if (actions.Contains("walk"))
			{
				actions.Add("jump");
			}
		}

		if (Input.IsActionJustPressed("interact"))
		{
			if (Humanoid.Resource.ItemFocus is not null)
			{
				actions.Add("interact");
			}
		}

		if (Input.IsActionJustPressed("unsheathe1"))
		{
			combatActions.Add("unsheathe1");
		}

		if (Input.IsActionPressed("attack1"))
		{
			if (Humanoid.CurrentState.StateName.Contains("slash1"))
			{
				combatActions.Add("attack1");
			}
			else
			{
				combatActions.Add("slash_prepare");
			}
		}

		if (Input.IsActionJustReleased("attack1"))
		{
			combatActions.Add("attack1");
		}

		if (Input.IsActionJustPressed("attack2"))
		{
			if (Humanoid.CurrentState.StateName.Contains("prepare") || combatActions.Contains("slash_prepare"))
			{
				combatActions.Add("attack3");
			}
			else
			{
				combatActions.Add("attack2");
			}
		}

		PriorityQueue<State, int> sortedActions = new(new DescendingComparer());
		PriorityQueue<string, int> sortedCombatActions = new(new DescendingComparer());

		SortActions(actions, sortedActions);
		SortCombatActions(combatActions, sortedCombatActions);

		return new InputPackage(sortedActions, sortedCombatActions, inputDirection);
	}

	private void SortActions(List<string> actionNames, PriorityQueue<State, int> sorted)
	{
		foreach (string action in actionNames)
		{
			State state = GetState(action);
			sorted.Enqueue(state, state.Priority);
		}
	}

	private void SortCombatActions(List<string> actionNames, PriorityQueue<string, int> sorted)
	{
		foreach (string action in actionNames)
		{
			sorted.Enqueue(action, _combatActionPriorities[action]);
		}
	}

	private State GetState(string name) => Humanoid.StateContainer.GetStateByName(name);
}

public struct InputPackage(PriorityQueue<State, int> actions, PriorityQueue<string, int> combatActionNames, Vector2 direction)
{
	public PriorityQueue<State, int> Actions { get; set; } = actions;
	public PriorityQueue<string, int> CombatActionNames { get; set; } = combatActionNames;
	public Vector2 Direction { get; set; } = direction;
}

public class DescendingComparer : IComparer<int>
{
	public int Compare(int x, int y) => y.CompareTo(x);
}