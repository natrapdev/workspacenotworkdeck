using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.states;

public partial class InputGatherer : Node
{
	[Export] public HumanoidModel Humanoid { get; set; }
	private float _startTime, _endTime, _avgTime, _shortTime, _longTime;

	private readonly Dictionary<string, int> _combatActionPriorities = new(6)
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

	protected readonly List<string> Actions = new(5);
	private readonly List<string> _combatActions = new(5);
	protected Vector2 InputDirection;

	public virtual InputPackage GatherInput()
	{
		Actions.Clear();
		_combatActions.Clear();

		GetInputs();

		return OnInputGathered();
	}

	private InputPackage OnInputGathered()
	{
		PriorityQueue<State, int> sortedActions = new(new DescendingComparer());
		PriorityQueue<string, int> sortedCombatActions = new(new DescendingComparer());

		SortActions(Actions, sortedActions);
		SortCombatActions(_combatActions, sortedCombatActions);

		return new InputPackage(sortedActions, sortedCombatActions, InputDirection);
	}

	protected virtual void GetInputs()
	{
		Actions.Add("idle");

		InputDirection = Input.GetVector("move_right", "move_left", "move_back", "move_forward");

		if (InputDirection != Vector2.Zero)
		{
			Actions.Add("walk");
		}

		if (Input.IsActionJustPressed("jump"))
		{
			if (Actions.Contains("walk"))
			{
				Actions.Add("jump");
			}
		}

		if (Input.IsActionJustPressed("interact"))
		{
			if (Humanoid.Resource.ItemFocus is not null)
			{
				Actions.Add("interact");
			}
		}

		if (Input.IsActionJustPressed("unsheathe1"))
		{
			_combatActions.Add("unsheathe1");
		}

		if (Input.IsActionPressed("attack1"))
		{
			if (Humanoid.CurrentState.StateName.Contains("slash1"))
				_combatActions.Add("attack1");
			else
				_combatActions.Add("slash_prepare");
		}

		if (Input.IsActionJustReleased("attack1"))
		{
			_combatActions.Add("attack1");
		}

		if (Input.IsActionJustPressed("attack2"))
		{
			if (Humanoid.CurrentState.StateName.Contains("prepare") || _combatActions.Contains("slash_prepare"))
			{
				_combatActions.Add("attack3");
			}
			else
			{
				_combatActions.Add("attack2");
			}
		}
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

public readonly struct InputPackage(
	PriorityQueue<State, int> actions,
	PriorityQueue<string, int> combatActionNames,
	Vector2 direction
	)
{
	public PriorityQueue<State, int> Actions { get; } = actions;
	public PriorityQueue<string, int> CombatActionNames { get; } = combatActionNames;
	public Vector2 Direction { get; } = direction;
}

public class DescendingComparer : IComparer<int>
{
	public int Compare(int x, int y) => y.CompareTo(x);
}