using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class RaiseSwordOneHanded : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public Task LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    public override State ChangeState(InputPackage input)
    {
        return !Character.IsOnFloor() ? Parent.GetStateByName("airborne") : base.ChangeState(input);
    }

    protected override void OnUpdate(InputPackage input, float delta)
    {
        // if (LegBehaviour.CurrentState.TracksLookDirection() && !TracksLookDirection()) LegsTrackLookDirection(input, delta);
        LegsUpdate(input, delta);
    }

    protected override void OnEnter()
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
    }
}
