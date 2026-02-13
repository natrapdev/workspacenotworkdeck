using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Jump : State
{
	private const float JUMP_IMPULSE = 2.5f;

	public override State ChangeState(InputPackage input)
	{
		if (!Character.IsOnFloor())
		{
			return Parent.GetStateByName("airborne");
		}

		return FindFirstValidState(input);
	}

	public override void OnUpdate(InputPackage input, float delta)
	{
		Character.Velocity -= new Vector3(0, Gravity * delta, 0);
	}

	public override void OnEnter()
	{
		Character.Velocity += new Vector3(0, JUMP_IMPULSE, 0);
	}
}
