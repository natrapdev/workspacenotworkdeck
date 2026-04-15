using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedSlashDown : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public Task LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    public override void OnUpdate(InputPackage input, float delta)
    {
        LegsUpdate(input, delta);
    }

    public override void OnEnter()
    {
        if (!FollowUpStates.Contains(Parent.GetStateByName("thrust_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("thrust_one_handed"));
        }
    }
}
