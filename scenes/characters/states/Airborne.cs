using Godot;
using System;
using System.Diagnostics;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Airborne : CharacterState
{
    const float GRAVITY = 9.8f;
    const float MAX_FALL_RECOVERY_DISTANCE = 8f;
    private float airTime;
    private readonly Stopwatch _stopwatch = new();

    public override string CheckRelevance(InputPackage input)
	{
		if (Character.IsOnFloor())
		{
			return FindFirstValidState(input);
		}

		return "OK";
	}

	public override void Update(InputPackage input, float delta)
	{
		Character.Velocity -= new Vector3(0, GRAVITY * delta, 0);
        airTime = _stopwatch.ElapsedMilliseconds;
	}

	public override void OnEnterState()
	{
		_stopwatch.Start();
	}
	public override void OnExitState()
	{
		_stopwatch.Stop();
		GD.Print("Airtime: " + airTime);
	}
}
