using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Airborne : CharacterState
{
    const float GRAVITY = 9.8f;
    const float MAX_FALL_RECOVERY_DISTANCE = 8f;
    private float airTime;
    private double startTime, endTime;

    public override string CheckRelevance(InputPackage input)
	{
		if (character.IsOnFloor())
		{
			Array.Sort(input.actions.ToArray(), statePriorities.ToArray());
			return input.actions[0];
		}

		return "OK";
	}

	public override void Update(InputPackage input, float delta)
	{
		character.Velocity -= new Vector3(0, GRAVITY * delta, 0);
        endTime = Time.GetUnixTimeFromSystem();
        airTime = (float)(endTime - startTime);
	}

	public override void OnEnterState()
	{
		startTime = Time.GetUnixTimeFromSystem();
	}
	public override void OnExitState()
	{
		GD.Print("Airtime: " + airTime);
	}
}
