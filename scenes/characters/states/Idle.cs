using System;
using System.Linq;
using Godot;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Idle : CharacterState
{
	private string _animation = "idle_";


	public override string CheckRelevance(InputPackage input)
	{
		if (!character.IsOnFloor())
		{
			return "airborne";
		}
		return FindFirstValidState(input);
	}

    public override void OnEnterState()
    {
        character.Velocity = Vector3.Zero;
    }

}