using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class RaiseSwordOneHanded : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public void LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    public override State ChangeState(InputPackage input)
    {
        if (!Character.IsOnFloor())
		{
			return Parent.GetStateByName("airborne");
		}
		return base.ChangeState(input);
    }

    public override void OnUpdate(InputPackage input, float delta)
    {
        // if (LegBehaviour.CurrentState.TracksLookDirection() && !TracksLookDirection()) LegsTrackLookDirection(input, delta);
        LegsUpdate(input, delta);
    }

    public override void OnEnter()
    {
        // NextState??=Parent.GetStateByName("slash1_one_handed");
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash1_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash1_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash3_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash3_one_handed"));
        }

        Animator.SetBodySpeedScale(Mathf.Clamp(Resource.CurrentStamina * 1.3f, 0.125f, 2));
    }

    public override void OnExit()
    {
        Animator.SetBodySpeedScale(1);
    }
}
