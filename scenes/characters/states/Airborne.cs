using Godot;
using System;
using System.Diagnostics;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Airborne : State
{
    const float GRAVITY = 9.8f;
    const float MAX_FALL_RECOVERY_DISTANCE = 8f;

	private Vector3 _enterPos, _landPos;

    public override State ChangeState(InputPackage input)
	{
		if (Character.IsOnFloor())
		{
			return FindFirstValidState(input);
		}

		return this;
	}

	protected override void OnUpdate(InputPackage input, float delta)
	{
		Character.Velocity -= new Vector3(0, GRAVITY * delta, 0);
	}

	protected override void OnEnter()
	{
		_enterPos = Character.GlobalPosition;
	}
	protected override void OnExit()
	{
		_landPos = Character.GlobalPosition;

		float displacement = _enterPos.DistanceTo(_landPos);

		GD.Print("Airtime: " + ElapsedTimeSeconds + " seconds");
		GD.Print("Displacement: " + displacement);
		GD.Print("Landing velocity: " + Character.Velocity.Length());
	}
}