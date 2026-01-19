using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Jump : CharacterState
{
	private const float JUMP_IMPULSE = 2.5f;
	private const float GRAVITY = 9.8f;
	public override string CheckRelevance(InputPackage input)
	{
		if (!character.IsOnFloor())
		{
			return "airborne";
		}

		return FindFirstValidState(input);
	}

	public override void Update(InputPackage input, float delta)
	{
		character.Velocity -= new Vector3(0, GRAVITY * delta, 0);
	}

	public override void OnEnterState()
	{
		character.Velocity += new Vector3(0, JUMP_IMPULSE, 0);
	}
	public override void OnExitState()
	{
		
	}
}
