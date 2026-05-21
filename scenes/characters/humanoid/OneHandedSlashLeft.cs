using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedSlashLeft : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    public void LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);

    protected override void OnUpdate(InputPackage input, float delta)
    {
        LegsUpdate(input, delta);
    }
}
