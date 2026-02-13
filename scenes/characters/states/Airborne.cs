using Godot;
using System;
using System.Diagnostics;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Airborne : State
{
    const float GRAVITY = 9.8f;
    const float MAX_FALL_RECOVERY_DISTANCE = 8f;
    private float _airTime;
    private readonly Stopwatch _stopwatch = new();

    public override State ChangeState(InputPackage input)
	{
		if (Character.IsOnFloor())
		{
			return FindFirstValidState(input);
		}

		return this;
	}

	public override void OnUpdate(InputPackage input, float delta)
	{
		Character.Velocity -= new Vector3(0, GRAVITY * delta, 0);
        _airTime = _stopwatch.ElapsedMilliseconds;
	}

	public override void OnEnter()
	{
		_stopwatch.Start();
	}
	public override void OnExit()
	{
		_stopwatch.Stop();
		GD.Print("Airtime: " + _airTime);
		_stopwatch.Reset();
	}
}
