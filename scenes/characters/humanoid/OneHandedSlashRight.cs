using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedSlashRight : State, IChildState, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }
    [Export] public State BaseState { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public Task LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    protected override void OnUpdate(InputPackage input, float delta)
    {
        LegsUpdate(input, delta);
    }

    protected override void OnEnter()
    {
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash3_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash3_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("thrust_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("thrust_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("idle")))
        {
            FollowUpStates.Add(Parent.GetStateByName("idle"));
        }
    }
}
